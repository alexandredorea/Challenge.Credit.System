namespace Challenge.Credit.System.Module.CreditCard.Core.Domain.Entities;

public sealed record CartaoCredito(
    Guid ProposalId,
    Guid ClientId,
    string ClientName,
    string ClientDocumentNumber,
    string CardNumber,
    string Cvv,
    DateTime ExpirationDate,
    decimal AvaliableLimit,
    decimal TotalLimit,
    CardStatus Status,
    DateTime? ActivationDate = null,
    DateTime? BlockDate = null)
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public DateTime IssueDate { get; init; } = DateTime.UtcNow;
}

public enum CardStatus
{
    Issued = 0,
    Activated = 1,
    Blocked = 2,
    Canceled = 3
}