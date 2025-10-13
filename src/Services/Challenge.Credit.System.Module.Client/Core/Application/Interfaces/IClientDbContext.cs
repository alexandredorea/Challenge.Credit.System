using Challenge.Credit.System.Shared.Outbox;
using Microsoft.EntityFrameworkCore;

namespace Challenge.Credit.System.Module.Client.Core.Application.Interfaces;

public interface IClientDbContext
{
    DbSet<Domain.Entities.Client> Clients { get; }

    DbSet<OutboxEvent> OutboxEvents { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken);
}