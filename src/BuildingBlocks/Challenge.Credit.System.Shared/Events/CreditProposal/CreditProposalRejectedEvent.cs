namespace Challenge.Credit.System.Shared.Events.CreditProposal;

public sealed record CreditProposalRejectedEvent(
    Guid Id,
    Guid ClientId,
    string ClientName,
    string ClientDocumentNumber,
    int Score,
    string RejectionReason,
    DateTime RejectionDate);