namespace Challenge.Credit.System.Shared.Events.CreditProposals;

public sealed record CreditProposalRejectedEvent(
    Guid ProposalId,
    Guid ClientId,
    string ClientName,
    int Score,
    string RejectionReason,
    DateTime RejectionDate);