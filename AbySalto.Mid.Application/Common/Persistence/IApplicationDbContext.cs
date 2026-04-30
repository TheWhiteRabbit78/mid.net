using AbySalto.Mid.Domain.Baskets;
using AbySalto.Mid.Domain.Favorites;
using AbySalto.Mid.Domain.Identity;
using Microsoft.EntityFrameworkCore;

namespace AbySalto.Mid.Application.Common.Persistence
{
    /// <summary>
    /// Application abstraction over the EF Core database context. Exposes only the entity sets
    /// that application services need — Identity tables remain accessible via UserManager.
    /// </summary>
    public interface IApplicationDbContext
    {
        DbSet<Favorite> Favorites { get; }
        DbSet<Basket> Baskets { get; }
        DbSet<BasketItem> BasketItems { get; }
        DbSet<RefreshToken> RefreshTokens { get; }
        DbSet<ApplicationUser> Users { get; }

        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    }
}
