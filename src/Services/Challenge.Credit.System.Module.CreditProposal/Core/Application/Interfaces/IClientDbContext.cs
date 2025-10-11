using Challenge.Credit.System.Module.CreditProposal.Core.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Challenge.Credit.System.Module.CreditProposal.Core.Application.Interfaces;

public interface IProposalDbContext
{
    DbSet<Proposal> Proposals { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken);
}