using Challenge.Credit.System.Module.CreditCard.Core.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Challenge.Credit.System.Module.CreditCard.Core.Application.Interfaces;

internal interface ICardDbContext
{
    DbSet<Card> Cards { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken);
}