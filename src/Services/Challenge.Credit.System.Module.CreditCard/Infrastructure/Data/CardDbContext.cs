using Challenge.Credit.System.Module.CreditCard.Core.Application.Interfaces;
using Challenge.Credit.System.Module.CreditCard.Core.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Challenge.Credit.System.Module.CreditCard.Infrastructure.Data;

internal sealed class CardDbContext(DbContextOptions<CardDbContext> options)
    : DbContext(options), ICardDbContext
{
    public DbSet<Card> Cards => Set<Card>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Card>(entity =>
        {
            entity.HasKey(e => e.Id);

            entity.Property(e => e.ClientName).IsRequired().HasMaxLength(200).IsUnicode(false);
            entity.Property(e => e.Number).IsRequired().HasMaxLength(16).IsUnicode(false);
            entity.Property(e => e.Cvv).IsRequired().HasMaxLength(3).IsUnicode(false);
            entity.Property(e => e.AvailableLimit).HasPrecision(18, 2);
            entity.Property(e => e.TotalLimit).HasPrecision(18, 2);
            entity.Property(e => e.Status).HasConversion<string>().HasMaxLength(10).IsUnicode(false);

            entity.HasIndex(e => e.ClientId);
            entity.HasIndex(e => e.ProposalId);
            entity.HasIndex(e => e.Number).IsUnique();
        });
    }
}