
namespace Challenge.Credit.System.Module.Client.Core.Application.DataTransferObjects;

public sealed record ClientResponse(
    Guid Id,
    string Name,
    string DocumentNumber,
    string Email,
    string Telephone,
    DateTime DateBirth,
    decimal MonthlyIncome,
    DateTime CreatedAt)
{
    public static implicit operator ClientResponse?(Domain.Entities.Client? client)
    {
        if (client is null)
            return null;

        return new(
            Id: client.Id,
            Name: client.Name,
            DocumentNumber: client.DocumentNumber,
            Email: client.Email,
            Telephone: client.Telephone,
            DateBirth: client.DateBirth,
            MonthlyIncome: client.MonthlyIncome,
            CreatedAt: client.CreatedAt);
    }
}
