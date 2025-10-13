using Challenge.Credit.System.Module.CreditProposal.Core.Application.Interfaces;
using Challenge.Credit.System.Module.CreditProposal.Core.Domain.Entities;
using Challenge.Credit.System.Module.CreditProposal.Core.Domain.Interfaces;
using Challenge.Credit.System.Module.CreditProposal.Core.Domain.ValueObjects;
using Challenge.Credit.System.Shared.Events.Clients;
using Challenge.Credit.System.Shared.Events.CreditProposals;
using Challenge.Credit.System.Shared.Outbox;

namespace Challenge.Credit.System.Module.CreditProposal.Core.Application.Services;

public interface IProposalService
{
    Task HandleAsync(ClientCreatedEvent @event, CancellationToken cancellationToken = default);

    //TODO: adicionar metodos Onter Por Id, Obter Propostas por Cliente, Listar todas as proposta
}

internal sealed class ProposalService(
    IProposalDbContext context,
    IScoreCalculator scoreCalculator,
    IScoreEvaluator scoreEvaluator,
    IOutboxService outboxService) : IProposalService
{
    public async Task HandleAsync(ClientCreatedEvent @event, CancellationToken cancellationToken = default)
    {
        var score = scoreCalculator.Calculate(
            @event.MonthlyIncome,
            new DateBirth(@event.DateBirth));

        var proposal = Proposal.Create(
            clientId: @event.ClientId,
            clientName: @event.ClientName,
            monthlyIncome: @event.MonthlyIncome,
            score: score);

        scoreEvaluator.Evaluate(proposal);

        context.Proposals.Add(proposal);

        if (proposal.Status == StatusProposal.Approved)
        {
            var proposalApproved = new CreditProposalApprovedEvent(
                ProposalId: proposal.Id,
                ClientId: proposal.ClientId,
                ClientName: proposal.ClientName,
                Score: proposal.Score,
                AvaliableLimit: proposal.AvaliableLimit,
                CardsAllowed: proposal.CardsAllowed,
                ApprovalDate: proposal.EvaluationDate);
            outboxService.AddEvent(proposalApproved);
        }

        await context.SaveChangesAsync(cancellationToken);
    }
}