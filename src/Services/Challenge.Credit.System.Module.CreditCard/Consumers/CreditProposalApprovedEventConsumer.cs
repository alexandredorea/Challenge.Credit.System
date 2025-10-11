using System.Text.Json;
using Challenge.Credit.System.Module.CreditCard.Core.Application.Services;
using Challenge.Credit.System.Shared.Events.CreditProposals;
using Challenge.Credit.System.Shared.Messaging.Interfaces;
using Microsoft.Extensions.Logging;

namespace Challenge.Credit.System.Module.CreditCard.Consumers;

public sealed class CreditProposalApprovedEventConsumer(
    ICardService cardService,
    ILogger<CreditProposalApprovedEventConsumer> logger) : IMessageConsumer
{
    public async Task ConsumeAsync(string message, CancellationToken cancellationToken = default)
    {
        logger.LogDebug("Processando evento de proposta aprovada");

        var @event = JsonSerializer.Deserialize<CreditProposalApprovedEvent>(message);

        if (@event is null)
        {
            logger.LogWarning("Evento de proposta aprovada inválido");
            return;
        }

        await cardService.HandleAsync(@event, cancellationToken);

        logger.LogDebug("Evento de proposta aprovada processado com sucesso para PropostaId: {PropostaId}", @event.ProposalId);
    }
}