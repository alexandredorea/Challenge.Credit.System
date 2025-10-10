using Challenge.Credit.System.Module.Client.Core.Application.Interfaces;
using Challenge.Credit.System.Module.CreditProposal.Core.Domain.Entities;
using Challenge.Credit.System.Module.CreditProposal.Core.Domain.Interfaces;
using Challenge.Credit.System.Module.CreditProposal.Core.Domain.ValueObjects;
using Challenge.Credit.System.Shared.Events.Clients;
using Challenge.Credit.System.Shared.Events.CreditProposals;
using Challenge.Credit.System.Shared.Messaging.Interfaces;

namespace Challenge.Credit.System.Module.CreditProposal.Core.Application.Services;

public interface IPropostaService
{
    Task HandleAsync(ClientCreatedEvent @event, CancellationToken cancellationToken = default);
    //TODO: adicionar metodos Onter Por Id, Obter Propostas por Cliente, Listar todas as proposta
}

public sealed class ProposalService(
    IProposalDbContext context,
    IScoreCalculator scoreCalculator,
    IScoreEvaluator scoreEvaluator,
    IMessagePublisher messagePublisher) : IPropostaService
{
    public async Task HandleAsync(ClientCreatedEvent @event, CancellationToken cancellationToken = default)
    {
        var score = scoreCalculator.Calculate(@event.MonthlyIncome, new DateBirth(@event.DateBirth));
        var proposal = Proposal.Create(
            clientId: @event.Id,
            clientName: @event.Name,
            clientDocumentNumber: @event.DocumentNumber,
            monthlyIncome: @event.MonthlyIncome,
            score: score);
        
        scoreEvaluator.Evaluate(proposal);

        await context.Proposals.AddAsync(proposal, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);

        // TODO: Usar Domain Events, Factory ou ja implementar direto Implementar Outbox Pattern (SAGA) para evitar salvar o proposta e nao gerar cartao em caso do rabbit estar fora do ar?
        if (proposal.Status == StatusProposal.Approved)
        {
            var proposalApproved = new CreditProposalApprovedEvent(
                Id: proposal.Id,
                ClientId: proposal.ClientId,
                ClientName: proposal.ClientName,
                ClientDocumentNumber: proposal.ClientDocumentNumber,
                Score: proposal.Score,
                AvaliableLimit: proposal.AvaliableLimit,
                CardsAllowed: proposal.CardsAllowed,
                ApprovalDate: proposal.EvaluationDate);

            await messagePublisher.PublishAsync("proposta.aprovada", proposalApproved, cancellationToken);
        }
        else
        {
            var proposalRejected = new CreditProposalRejectedEvent(
                Id: proposal.Id,
                ClientId: proposal.ClientId,
                ClientName: proposal.ClientName,
                ClientDocumentNumber: proposal.ClientDocumentNumber,
                Score: proposal.Score, 
                proposal.RejectionReason!, 
                proposal.EvaluationDate);

            await messagePublisher.PublishAsync("proposta.reprovada", proposalRejected, cancellationToken);
        }
    }
}
