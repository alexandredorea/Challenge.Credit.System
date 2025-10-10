namespace Challenge.Credit.System.Module.Client.Core.Application.Interfaces;

using Challenge.Credit.System.Module.CreditProposal.Core.Domain.Entities;
using Microsoft.EntityFrameworkCore;

public interface IProposalDbContext
{
    DbSet<Proposal> Proposals { get; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken);
}