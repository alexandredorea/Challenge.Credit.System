using Challenge.Credit.System.Module.Client.Core.Application.DataTransferObjects;
using Challenge.Credit.System.Module.Client.Core.Application.Interfaces;
using Challenge.Credit.System.Shared.Events.Clients;
using Challenge.Credit.System.Shared.Outbox;
using Microsoft.EntityFrameworkCore;

namespace Challenge.Credit.System.Module.Client.Core.Application.Services;

public interface IClientService
{
    Task<IEnumerable<ClientResponse?>> GetAllAsync(CancellationToken cancellationToken = default);

    Task<ClientResponse?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<ClientResponse?> CreateAsync(CreateClientRequest request, CancellationToken cancellationToken = default);
}

internal sealed class ClientService(
    IClientDbContext context,
    IOutboxService outboxService) : IClientService
{
    public async Task<IEnumerable<ClientResponse?>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        //TODO: paginacao
        var clients = await context.Clients
            .OrderByDescending(c => c.CreatedAt)
            .ToListAsync(cancellationToken);

        return clients.Select(item => (ClientResponse?)item).ToList();
    }

    public async Task<ClientResponse?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await context.Clients.FindAsync([id], cancellationToken);
    }

    public async Task<ClientResponse?> CreateAsync(CreateClientRequest request, CancellationToken cancellationToken = default)
    {
        var documentExists = await context.Clients.AnyAsync(c => c.Document.Number == request.DocumentNumber, cancellationToken);
        if (documentExists)
            return null;

        var emailExists = await context.Clients.AnyAsync(c => c.Email == request.Email, cancellationToken);
        if (emailExists)
            return null;

        var client = Domain.Entities.Client.Create(
        request.Name,
        request.DocumentNumber,
        request.Email,
        request.Telephone,
        request.DateBirth,
        request.MonthlyIncome);

        var @event = new ClientCreatedEvent(
            client.Id,
            client.Name,
            client.MonthlyIncome,
            client.DateBirth);

        context.Clients.Add(client);
        outboxService.AddEvent(@event);
        await context.SaveChangesAsync(cancellationToken);

        return client;
    }
}