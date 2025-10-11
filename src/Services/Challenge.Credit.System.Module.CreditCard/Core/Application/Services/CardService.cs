using Challenge.Credit.System.Module.CreditCard.Core.Application.Interfaces;
using Challenge.Credit.System.Module.CreditCard.Core.Domain.Entities;
using Challenge.Credit.System.Shared.Events.CreditProposals;
using Microsoft.EntityFrameworkCore;

namespace Challenge.Credit.System.Module.CreditCard.Core.Application.Services;

public interface ICardService
{
    Task HandleAsync(CreditProposalApprovedEvent @event, CancellationToken cancellationToken = default);

    //TODO: Adicionar: obter cartao por id, obter lita de cartoes por client
}

internal sealed class CardService(ICardDbContext context, ICardGeneratorService cardGeneratorService) : ICardService
{
    public async Task HandleAsync(CreditProposalApprovedEvent @event, CancellationToken cancellationToken = default)
    {
        // Valida se já existem cartões para esta proposta (idempotência, evitar duplicidade)
        var cardExists = await context.Cards.AnyAsync(c => c.ProposalId == @event.ProposalId, cancellationToken);
        if (cardExists)
            return;

        for (int index = 0; index < @event.CardsAllowed; index++)
        {
            var card = Card.Create(
                @event.ProposalId,
                @event.ClientId,
                @event.ClientName,
                cardGeneratorService.GenerateCardNumber(),
                cardGeneratorService.GenerateCvv(),
                cardGeneratorService.GenerateExpirationDate(),
                @event.AvaliableLimit);

            context.Cards.Add(card);
        }

        await context.SaveChangesAsync(cancellationToken);
    }
}