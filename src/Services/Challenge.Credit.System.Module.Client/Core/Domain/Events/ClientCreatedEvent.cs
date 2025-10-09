namespace Challenge.Credit.System.Module.Client.Core.Domain.Events;

public sealed record ClientCreatedEvent(
    Guid Id,
    string Name,
    string DocumentNumber,
    string Email,
    decimal MonthlyIncome,
    string Telephone,
    DateTime DateBirth,
    DateTime CreatedAt);