using System.Text.Json;
using Challenge.Credit.System.Module.CreditProposal.Core.Application.Services;
using Challenge.Credit.System.Shared.Events.Clients;
using Challenge.Credit.System.Shared.Messaging.Interfaces;
using Microsoft.Extensions.Logging;

namespace Challenge.Credit.System.Module.CreditProposal.Consumers;

internal sealed class ClientCreatedEventConsumer(
    IProposalService proposalService,
    ILogger<ClientCreatedEventConsumer> logger) : IMessageConsumer
{
    public async Task ConsumeAsync(string message, CancellationToken cancellationToken = default)
    {
        logger.LogDebug("Processando evento de proposta aprovada");

        var evento = JsonSerializer.Deserialize<ClientCreatedEvent>(message);

        if (evento is null)
        {
            logger.LogDebug("Evento de proposta aprovada inválido");
            return;
        }

        await proposalService.HandleAsync(evento, cancellationToken);

        logger.LogDebug("Evento de proposta aprovada processado com sucesso para PropostaId: {PropostaId}", evento.Id);
    }
}