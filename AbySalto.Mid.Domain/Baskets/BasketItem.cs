using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AbySalto.Mid.Domain.Baskets
{
    /// <summary>
    /// Represents a single line item within a <see cref="Basket"/>. 
    /// The product itself is not stored locally
    /// </summary>
    public class BasketItem
    {
        public int Id { get; set; }

        public int BasketId { get; set; }

        public int ProductId { get; set; }

        public int Quantity { get; set; }

        public DateTime AddedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Navigation property for the parent <see cref="Basket"/>.
        /// </summary>
        public virtual Basket? BasketFK { get; set; }
    }

    public class BasketItemConfiguration : IEntityTypeConfiguration<BasketItem>
    {
        public void Configure(EntityTypeBuilder<BasketItem> builder)
        {
            builder.HasKey(bi => bi.Id);

            builder.HasOne(bi => bi.BasketFK)
                   .WithMany(b => b.BasketItemsFK)
                   .HasForeignKey(bi => bi.BasketId)
                   .OnDelete(DeleteBehavior.Cascade)
                   .IsRequired(true);

            // Prevent duplicate product entries within the same basket — quantity is updated instead
            builder.HasIndex(bi => new { bi.BasketId, bi.ProductId }).IsUnique();
        }
    }
}
