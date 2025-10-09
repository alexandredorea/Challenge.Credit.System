namespace Challenge.Credit.System.Module.Client.Core.Domain.Entities;

public sealed record Client(
    string Name,
    string DocumentNumber,
    string Email,
    string Telephone,
    DateTime DateBirth,
    decimal MonthlyIncome)
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
