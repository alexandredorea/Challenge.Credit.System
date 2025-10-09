namespace Challenge.Credit.System.Module.Client.Core.Application.Interfaces;

using Microsoft.EntityFrameworkCore;

public interface IClientDbContext
{
    DbSet<Domain.Entities.Client> Clients { get; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken);
}