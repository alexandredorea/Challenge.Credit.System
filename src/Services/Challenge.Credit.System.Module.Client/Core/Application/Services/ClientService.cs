using Challenge.Credit.System.Module.Client.Core.Application.DataTransferObjects;
using Challenge.Credit.System.Module.Client.Core.Application.Interfaces;
using Challenge.Credit.System.Module.Client.Core.Domain.Events;
using Challenge.Credit.System.Shared.Messaging.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Challenge.Credit.System.Module.Client.Core.Application.Services;

public interface IClientService
{
    Task<List<ClientResponse?>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<ClientResponse?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<ClientResponse?> CreateAsync(CreateClientRequest request, CancellationToken cancellationToken = default);
}

internal sealed class ClientService(IClientDbContext context, IMessagePublisher messagePublisher) : IClientService
{
    public async Task<List<ClientResponse?>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var clients = await context.Clients
            .OrderByDescending(c => c.CreatedAt)
            .ToListAsync(cancellationToken);

        return clients.Select(item => (ClientResponse?)item).ToList();
    }

    public async Task<ClientResponse?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await context.Clients.FindAsync(new object[] { id }, cancellationToken);
    }

    public async Task<ClientResponse?> CreateAsync(CreateClientRequest request, CancellationToken cancellationToken = default)
    {
        var documentNumberExists = await context.Clients.AnyAsync(c => c.DocumentNumber == request.DocumentNumber, cancellationToken);
        if (documentNumberExists)
            return null;

        var emailExists = await context.Clients.AnyAsync(c => c.Email == request.Email, cancellationToken);
        if (emailExists)
            return null;

        var client = new Domain.Entities.Client(request.Name, request.DocumentNumber, request.Email, request.Telephone, request.DateBirth, request.MonthlyIncome);

        context.Clients.Add(client);
        await context.SaveChangesAsync(cancellationToken);

        // TODO: Implementar Outbox Pattern para evitar salvar o cliente e nao gerar proposta em caso do rabbit estar fora do ar?
        var @event = new ClientCreatedEvent(
            client.Id,
            client.Name,
            client.DocumentNumber,
            client.Email,
            client.MonthlyIncome,
            client.Telephone,
            client.DateBirth,
            client.CreatedAt);

        await messagePublisher.PublishAsync(queueName: "cliente.cadastrado", message: @event, cancellationToken: cancellationToken);

        return client;
    }
}
