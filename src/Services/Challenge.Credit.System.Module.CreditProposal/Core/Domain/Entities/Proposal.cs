namespace Challenge.Credit.System.Module.CreditProposal.Core.Domain.Entities;

public sealed record Proposal(
    Guid ClientId,
    string ClientName,
    string ClientDocumentNumber,
    decimal MonthlyIncome,
    int Score,
    StatusProposal Status,
    decimal AvaliableLimit,
    int CardsAllowed,
    string RejectionReason,
    DateTime? EvaluationDate)
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public DateTime CreatedAt { get; init; } = DateTime.UtcNow;
}

public enum StatusProposal
{
    Pending = 0,
    Approved = 1,
    Failed = 2
}
