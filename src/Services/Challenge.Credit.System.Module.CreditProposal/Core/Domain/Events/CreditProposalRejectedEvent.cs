namespace Challenge.Credit.System.Module.CreditProposal.Core.Domain.Events;

public sealed record CreditProposalRejectedEvent(
    Guid Id,
    Guid ClientId,
    string ClientName,
    string ClientDocumentNumber,
    int Score,
    string RejectionReason,
    DateTime RejectionDate);