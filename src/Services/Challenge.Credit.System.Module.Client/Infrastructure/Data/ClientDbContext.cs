using Challenge.Credit.System.Module.Client.Core.Application.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Challenge.Credit.System.Module.Client.Infrastructure.Data;

internal sealed class ClientDbContext(DbContextOptions<ClientDbContext> options) 
    : DbContext(options), IClientDbContext
{
    public DbSet<Core.Domain.Entities.Client> Clients => Set<Core.Domain.Entities.Client>();
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Core.Domain.Entities.Client>(entity =>
        {
            entity.HasKey(e => e.Id);

            entity.Property(e => e.Name).IsRequired().HasMaxLength(200).IsUnicode(false);
            entity.Property(e => e.DocumentNumber).IsRequired().HasMaxLength(11).IsUnicode(false);
            entity.Property(e => e.Email).IsRequired().HasMaxLength(254).IsUnicode(false); //RFC 3696, section-3
            entity.Property(e => e.Telephone).IsRequired().HasMaxLength(11).IsUnicode(false);
            entity.Property(e => e.MonthlyIncome).HasPrecision(18, 2).IsUnicode(false);

            entity.HasIndex(e => e.DocumentNumber).IsUnique();
            entity.HasIndex(e => e.Email).IsUnique();
        });
    }
}
