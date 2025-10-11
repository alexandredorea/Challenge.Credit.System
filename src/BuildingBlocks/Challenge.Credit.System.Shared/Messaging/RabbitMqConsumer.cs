using System.Text;
using Challenge.Credit.System.Shared.Messaging.Interfaces;
using Challenge.Credit.System.Shared.Policies;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Challenge.Credit.System.Shared.Messaging;

internal sealed class RabbitMqConsumer(
    IServiceProvider serviceProvider,
    ILogger<RabbitMqConsumer> logger,
    string hostName,
    string queueName,
    string exchangeName,
    string routingKey,
    Type messageHandlerType) : BackgroundService, IAsyncDisposable
{
    private IChannel? _channel;
    private IConnection? _connection;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("Iniciando consumer resiliente para fila: {QueueName}", queueName);
        stoppingToken.Register(() => logger.LogInformation("Cancelamento solicitado para fila '{QueueName}'. Fechando conexão do consumer...", queueName));

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ConnectAndSetupAsync(stoppingToken);
                logger.LogInformation("Consumer conectado e escutando a fila '{QueueName}'.", queueName);

                // Mantém o método em execução aguardando até que o token de cancelamento seja acionado.
                // A lógica de consumo acontece nos eventos do canal.
                await Task.Delay(Timeout.Infinite, stoppingToken);
            }
            catch (OperationCanceledException)
            {
                // Exceção esperada quando a aplicação está parando. Não é um erro.
                //_logger.LogInformation(ex, "Processamento da mensagem cancelado devido ao desligamento do serviço. A mensagem retornará à fila '{QueueName}'.", _queueName);
                break;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Erro inesperado no Consumer da fila '{QueueName}'. Tentando reconectar em 5 segundos.", queueName);
                await CleanupConnection(); // Limpa os recursos antes de tentar reconectar
                await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
            }
        }

        logger.LogInformation("O consumer para a fila '{QueueName}' foi finalizado.", queueName);
    }

    private async Task ConnectAndSetupAsync(CancellationToken stoppingToken)
    {
        var factory = new ConnectionFactory
        {
            HostName = hostName,
            AutomaticRecoveryEnabled = true,
            NetworkRecoveryInterval = TimeSpan.FromSeconds(10),
        };

        _connection = await Task.Run(() => factory.CreateConnectionAsync(), stoppingToken);
        _channel = await _connection.CreateChannelAsync(cancellationToken: stoppingToken);

        // Configurar a topologia (exchanges, filas, bindings) de forma assíncrona
        await SetupTopologyAsync(_channel, stoppingToken);

        // Configurar QoS (Quality of Service)
        await _channel.BasicQosAsync(prefetchSize: 0, prefetchCount: 1, global: false, stoppingToken);

        var consumer = new AsyncEventingBasicConsumer(_channel);
        consumer.ReceivedAsync += OnMessageReceived;

        await _channel.BasicConsumeAsync(queue: queueName, autoAck: false, consumer: consumer, stoppingToken);
        logger.LogInformation("Consumer conectado e escutando a fila '{QueueName}'.", queueName);
    }

    private async Task OnMessageReceived(object sender, BasicDeliverEventArgs ea)
    {
        var messageBody = Encoding.UTF8.GetString(ea.Body.ToArray());
        logger.LogInformation("Mensagem recebida na fila {QueueName}. DeliveryTag: {DeliveryTag}", queueName, ea.DeliveryTag);

        var messageRetryPolicy = ResiliencePolicies.CreateMessagingRetryPolicy(logger);

        try
        {
            await messageRetryPolicy.ExecuteAsync(async () =>
            {
                // Cria um escopo de injeção de dependência para cada mensagem
                using var scope = serviceProvider.CreateScope();
                var messageHandler = scope.ServiceProvider.GetRequiredService(messageHandlerType) as IMessageConsumer;

                if (messageHandler is null)
                {
                    logger.LogCritical("Não foi possível resolver o handler do tipo '{HandlerType}'", messageHandlerType.Name);
                    throw new InvalidOperationException($"Não foi possível resolver o handler do tipo '{messageHandlerType.Name}'");
                }

                await messageHandler.ConsumeAsync(messageBody, CancellationToken.None); // TODO: usar o CancellationToken para handler
            });

            // Se a política de retry for bem-sucedida, envia o ACK
            await _channel!.BasicAckAsync(ea.DeliveryTag, false);
            logger.LogInformation("Mensagem da fila {QueueName} processada com sucesso (ACK). DeliveryTag: {DeliveryTag}", queueName, ea.DeliveryTag);
        }
        catch (Exception ex)
        {
            // Se a política de retry falhar, a exceção será capturada aqui
            logger.LogError(ex, "Erro ao processar mensagem da fila {QueueName}. Enviando para DLQ (NACK). DeliveryTag: {DeliveryTag}", queueName, ea.DeliveryTag);

            // Rejeita a mensagem (requeue: false) para que ela vá para a DLQ configurada
            await _channel!.BasicNackAsync(deliveryTag: ea.DeliveryTag, multiple: false, requeue: false);
        }
    }

    private async Task SetupTopologyAsync(IChannel channel, CancellationToken stoppingToken)
    {
        // Nomes para DLX e DLQ
        var dlxName = $"{exchangeName}.dlx";
        var dlqName = $"{queueName}.dlq";

        // Declarar Dead Letter Exchange
        await channel.ExchangeDeclareAsync(exchange: dlxName, type: ExchangeType.Direct, durable: true, cancellationToken: stoppingToken);
        // Declarar Dead Letter Queue
        await channel.QueueDeclareAsync(queue: dlqName, durable: true, exclusive: false, autoDelete: false, cancellationToken: stoppingToken);
        // Bind DLQ ao DLX
        await channel.QueueBindAsync(queue: dlqName, exchange: dlxName, routingKey: dlqName, cancellationToken: stoppingToken);
        // Declarar exchange principal
        await channel.ExchangeDeclareAsync(exchange: exchangeName, type: ExchangeType.Topic, durable: true, cancellationToken: stoppingToken);

        // Declarar fila principal com DLX configurado
        var arguments = new Dictionary<string, object?>
        {
            { "x-dead-letter-exchange", dlxName },
            { "x-dead-letter-routing-key", dlqName }
        };
        await channel.QueueDeclareAsync(queueName, durable: true, exclusive: false, autoDelete: false, arguments, cancellationToken: stoppingToken);

        // Bind da fila principal à exchange principal
        await channel.QueueBindAsync(queue: queueName, exchange: exchangeName, routingKey: routingKey, cancellationToken: stoppingToken);

        logger.LogInformation("Topologia do RabbitMQ para a fila '{QueueName}' configurada com sucesso.", queueName);
    }

    private async Task CleanupConnection()
    {
        if (_channel is not null)
        {
            try
            {
                await _channel.CloseAsync();
                _channel.Dispose();
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Erro ao fechar o canal RabbitMQ durante DisposeAsync.");
            }
            finally
            {
                _channel = null;
            }
        }

        if (_connection is not null)
        {
            try
            {
                await _connection.CloseAsync();
                _connection.Dispose();
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Erro ao fechar a conexão RabbitMQ durante DisposeAsync.");
            }
            finally
            {
                _connection = null;
            }
        }
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        logger.LogInformation("Parando consumer RabbitMQ...");
        await base.StopAsync(cancellationToken);
    }

    public async ValueTask DisposeAsync()
    {
        await CleanupConnection();
        GC.SuppressFinalize(this);
    }
}