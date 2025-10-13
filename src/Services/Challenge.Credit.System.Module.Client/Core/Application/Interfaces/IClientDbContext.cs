using Challenge.Credit.System.Shared.Outbox;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace Challenge.Credit.System.Module.Client.Core.Application.Interfaces;

public interface IClientDbContext
{
    DbSet<Domain.Entities.Client> Clients { get; }

    DbSet<OutboxEvent> OutboxEvents { get; }

    DatabaseFacade Database { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken);
}