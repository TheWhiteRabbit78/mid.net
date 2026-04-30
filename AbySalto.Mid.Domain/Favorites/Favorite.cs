using AbySalto.Mid.Domain.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AbySalto.Mid.Domain.Favorites
{
    /// <summary>
    /// Represents a single favorited product for a user. 
    /// </summary>
    public class Favorite
    {
        public int Id { get; set; }

        public string UserId { get; set; } = string.Empty;

        public int ProductId { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Navigation property for the related <see cref="ApplicationUser"/>.
        /// </summary>
        public virtual ApplicationUser? ApplicationUserFK { get; set; }
    }

    public class FavoriteConfiguration : IEntityTypeConfiguration<Favorite>
    {
        public void Configure(EntityTypeBuilder<Favorite> builder)
        {
            builder.HasKey(f => f.Id);

            builder.HasOne(f => f.ApplicationUserFK)
                   .WithMany(u => u.FavoritesFK)
                   .HasForeignKey(f => f.UserId)
                   .OnDelete(DeleteBehavior.Cascade)
                   .IsRequired(true);

            // Prevent duplicate favorites of the same product by the same user
            builder.HasIndex(f => new { f.UserId, f.ProductId }).IsUnique();
        }
    }
}
