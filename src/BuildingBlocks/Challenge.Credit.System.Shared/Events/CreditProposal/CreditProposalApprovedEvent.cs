namespace Challenge.Credit.System.Shared.Events.CreditProposal;

public sealed record CreditProposalApprovedEvent(
    Guid Id,
    Guid ClientId,
    string ClientName,
    string ClientDocumentNumber,
    int Score,
    decimal AvaliableLimit,
    int CardsAllowed,
    DateTime ApprovalDate);