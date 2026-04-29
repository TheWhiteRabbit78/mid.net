using AbySalto.Mid.Domain.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AbySalto.Mid.Domain.Baskets
{
    /// <summary>
    /// Represents a user's shopping basket. Each user has exactly one basket creatted on first use.
    /// </summary>
    public class Basket
    {
        public int Id { get; set; }

        public string UserId { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Navigation property for the related <see cref="ApplicationUser"/>.
        /// </summary>
        public virtual ApplicationUser? ApplicationUserFK { get; set; }

        /// <summary>
        /// Navigation property for the items currently in the basket.
        /// </summary>
        public virtual ICollection<BasketItem>? BasketItemsFK { get; set; }
    }

    public class BasketConfiguration : IEntityTypeConfiguration<Basket>
    {
        public void Configure(EntityTypeBuilder<Basket> builder)
        {
            builder.HasKey(b => b.Id);

            builder.HasOne(b => b.ApplicationUserFK)
                   .WithOne(u => u.BasketFK)
                   .HasForeignKey<Basket>(b => b.UserId)
                   .OnDelete(DeleteBehavior.Cascade)
                   .IsRequired(true);

            // Enforce single basket per user
            builder.HasIndex(b => b.UserId).IsUnique();
        }
    }
}
