using Challenge.Credit.System.Module.CreditProposal.Core.Domain.Entities;
using Challenge.Credit.System.Shared.Outbox;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace Challenge.Credit.System.Module.CreditProposal.Core.Application.Interfaces;

public interface IProposalDbContext
{
    DbSet<Proposal> Proposals { get; }

    DbSet<OutboxEvent> OutboxEvents { get; }

    DatabaseFacade Database { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken);
}