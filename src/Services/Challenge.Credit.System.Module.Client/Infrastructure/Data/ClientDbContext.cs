using Challenge.Credit.System.Module.Client.Core.Application.Interfaces;
using Challenge.Credit.System.Shared.Outbox;
using Microsoft.EntityFrameworkCore;

namespace Challenge.Credit.System.Module.Client.Infrastructure.Data;

public sealed class ClientDbContext(DbContextOptions<ClientDbContext> options)
    : DbContext(options), IClientDbContext
{
    public DbSet<Core.Domain.Entities.Client> Clients => Set<Core.Domain.Entities.Client>();
    public DbSet<OutboxEvent> OutboxEvents => Set<OutboxEvent>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Core.Domain.Entities.Client>(entity =>
        {
            entity.HasKey(e => e.Id);

            entity.Property(e => e.Name).IsRequired().HasMaxLength(200).IsUnicode(false);

            entity.OwnsOne(c => c.Document, doc =>
            {
                doc.Property(d => d.Number)
                    .HasColumnName("DocumentNumber")
                    .IsRequired()
                    .HasMaxLength(14)
                    .IsUnicode(false);

                doc.Property(d => d.Type)
                    .HasColumnName("DocumentType")
                    .IsRequired()
                    .HasMaxLength(5)
                    .HasConversion<string>()
                    .IsUnicode(false);

                doc.HasIndex(x => x.Number)
                    .IsUnique()
                    .HasDatabaseName("IX_Clients_DocumentNumber");
            });

            entity.Property(e => e.Email).IsRequired().HasMaxLength(254).IsUnicode(false);
            entity.Property(e => e.Telephone).IsRequired().HasMaxLength(11).IsUnicode(false);
            entity.Property(e => e.MonthlyIncome).HasPrecision(18, 2).IsUnicode(false);

            entity.HasIndex(e => e.Email).IsUnique().HasDatabaseName("IX_Clients_Email");
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