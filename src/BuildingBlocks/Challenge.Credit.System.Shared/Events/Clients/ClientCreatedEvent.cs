namespace Challenge.Credit.System.Shared.Events.Clients;

public sealed record ClientCreatedEvent(
    Guid Id,
    string Name,
    string DocumentNumber,
    string Email,
    decimal MonthlyIncome,
    string Telephone,
    DateTime DateBirth,
    DateTime CreatedAt);