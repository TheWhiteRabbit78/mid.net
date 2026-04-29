using AbySalto.Mid.Domain.Baskets;
using AbySalto.Mid.Domain.Favorites;
using AbySalto.Mid.Domain.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;

namespace AbySalto.Mid.Infrastructure.Persistence
{
    /// <summary>
    /// Application database context. Inherits from <see cref="IdentityDbContext{TUser, TRole, TKey}"/>
    /// to provide ASP.NET Core Identity tables alongside the application's own entities.
    /// </summary>
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser, ApplicationRole, string>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public virtual DbSet<Favorite> Favorites { get; set; } = null!;
        public virtual DbSet<Basket> Baskets { get; set; } = null!;
        public virtual DbSet<BasketItem> BasketItems { get; set; } = null!;
        public virtual DbSet<RefreshToken> RefreshTokens { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Apply all IEntityTypeConfiguration implementations from the Domain assembly
            builder.ApplyConfigurationsFromAssembly(typeof(ApplicationUser).Assembly);
        }
    }
}
