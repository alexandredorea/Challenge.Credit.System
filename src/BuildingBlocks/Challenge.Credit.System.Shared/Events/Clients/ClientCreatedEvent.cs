namespace Challenge.Credit.System.Shared.Events.Clients;

public sealed record ClientCreatedEvent(
    Guid ClientId,
    string ClientName,
    decimal MonthlyIncome,
    DateTime DateBirth);