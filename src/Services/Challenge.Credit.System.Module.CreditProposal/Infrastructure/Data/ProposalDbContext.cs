using Challenge.Credit.System.Module.CreditProposal.Core.Application.Interfaces;
using Challenge.Credit.System.Module.CreditProposal.Core.Domain.Entities;
using Challenge.Credit.System.Shared.Outbox;
using Microsoft.EntityFrameworkCore;

namespace Challenge.Credit.System.Module.CreditProposal.Infrastructure.Data;

internal sealed class ProposalDbContext(DbContextOptions<ProposalDbContext> options)
    : DbContext(options), IProposalDbContext
{
    public DbSet<Proposal> Proposals => Set<Proposal>();
    public DbSet<OutboxEvent> OutboxEvents => Set<OutboxEvent>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Proposal>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.ClientName).IsRequired().HasMaxLength(200);
            entity.Property(e => e.MonthlyIncome).HasPrecision(18, 2);
            entity.Property(e => e.AvaliableLimit).HasPrecision(18, 2);
            entity.Property(e => e.Status).HasConversion<string>();

            entity.HasIndex(e => e.ClientId);
            entity.HasIndex(e => e.Status);
        });

        modelBuilder.Entity<OutboxEvent>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.EventType).IsRequired().HasMaxLength(250);
            entity.Property(e => e.Payload).IsRequired();
            entity.Property(e => e.CreatedAt).IsRequired();
            entity.Property(e => e.Processed).IsRequired();
            entity.Property(e => e.RetryCount).IsRequired();
            entity.Property(e => e.ErrorMessage).HasMaxLength(2000);

            entity.HasIndex(e => new { e.Processed, e.RetryCount })
                .HasDatabaseName("IX_OutboxEvents_Processed_RetryCount");
            entity.HasIndex(e => e.CreatedAt)
                .HasDatabaseName("IX_OutboxEvents_CreatedAt");
        });
    }
}