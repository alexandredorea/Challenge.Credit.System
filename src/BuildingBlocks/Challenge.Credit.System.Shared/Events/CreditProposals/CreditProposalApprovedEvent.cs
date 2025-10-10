namespace Challenge.Credit.System.Shared.Events.CreditProposals;

public sealed record CreditProposalApprovedEvent(
    Guid Id,
    Guid ClientId,
    string ClientName,
    string ClientDocumentNumber,
    int Score,
    decimal AvaliableLimit,
    int CardsAllowed,
    DateTime ApprovalDate);