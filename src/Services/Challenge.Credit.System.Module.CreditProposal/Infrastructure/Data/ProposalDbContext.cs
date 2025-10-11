using Challenge.Credit.System.Module.CreditProposal.Core.Application.Interfaces;
using Challenge.Credit.System.Module.CreditProposal.Core.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Challenge.Credit.System.Module.CreditProposal.Infrastructure.Data;

internal sealed class ProposalDbContext(DbContextOptions<ProposalDbContext> options)
    : DbContext(options), IProposalDbContext
{
    public DbSet<Proposal> Proposals => Set<Proposal>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Proposal>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.ClientName).IsRequired().HasMaxLength(200);
            entity.Property(e => e.MonthlyIncome).HasPrecision(18, 2);
            entity.Property(e => e.AvaliableLimit).HasPrecision(18, 2);
            entity.Property(e => e.Status).HasConversion<int>();

            entity.HasIndex(e => e.ClientId);
            entity.HasIndex(e => e.Status);
        });
    }
}