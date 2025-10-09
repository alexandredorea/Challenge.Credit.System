namespace Challenge.Credit.System.Module.CreditProposal.Core.Domain.Events;

public sealed record CreditProposalApprovedEvent(
    Guid Id,
    Guid ClientId,
    string ClientName,
    string ClientDocumentNumber,
    int Score,
    decimal AvaliableLimit,
    int CardsAllowed,
    DateTime ApprovalDate);