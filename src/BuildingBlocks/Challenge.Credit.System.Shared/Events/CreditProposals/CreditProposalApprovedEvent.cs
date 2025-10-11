namespace Challenge.Credit.System.Shared.Events.CreditProposals;

public sealed record CreditProposalApprovedEvent(
    Guid ProposalId,
    Guid ClientId,
    string ClientName,
    int Score,
    decimal AvaliableLimit,
    int CardsAllowed,
    DateTime ApprovalDate);