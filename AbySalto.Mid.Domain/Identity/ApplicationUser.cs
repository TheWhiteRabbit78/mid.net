using System.ComponentModel.DataAnnotations;
using AbySalto.Mid.Domain.Baskets;
using AbySalto.Mid.Domain.Favorites;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AbySalto.Mid.Domain.Identity
{
    /// <summary>
    /// Application user that extends <see cref="IdentityUser"/> with profile fields
    /// and navigation to the user's basket, favorites and refresh token.
    /// </summary>
    public class ApplicationUser : IdentityUser
    {
        public ApplicationUser()
        {
            FirstName = string.Empty;
            LastName = string.Empty;
        }

        [PersonalData, StringLength(40)]
        public string FirstName { get; set; }

        [PersonalData, StringLength(40)]
        public string LastName { get; set; }

        public string FullName => $"{FirstName} {LastName}";

        /// <summary>
        /// Foreign key for the user's current refresh token (one-to-one).
        /// </summary>
        public int? RefreshTokenId { get; set; }

        /// <summary>
        /// Navigation property for the user's current refresh token.
        /// </summary>
        public virtual RefreshToken? RefreshTokenFK { get; set; }

        /// <summary>
        /// Navigation property for the user's basket. One basket per user.
        /// </summary>
        public virtual Basket? BasketFK { get; set; }

        /// <summary>
        /// Navigation property for the user's favorited products.
        /// </summary>
        public virtual ICollection<Favorite>? FavoritesFK { get; set; }
    }

    public class ApplicationUserConfiguration : IEntityTypeConfiguration<ApplicationUser>
    {
        public void Configure(EntityTypeBuilder<ApplicationUser> builder)
        {
            // 1:1 between ApplicationUser and RefreshToken — user holds the FK
            builder.HasOne(u => u.RefreshTokenFK)
                   .WithOne(rt => rt.ApplicationUserFK)
                   .HasForeignKey<ApplicationUser>(u => u.RefreshTokenId)
                   .OnDelete(DeleteBehavior.SetNull)
                   .IsRequired(false);
        }
    }
}
