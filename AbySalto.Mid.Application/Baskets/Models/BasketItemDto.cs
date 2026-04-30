using AbySalto.Mid.Application.Products.Models;

namespace AbySalto.Mid.Application.Baskets.Models
{
    /// <summary>
    /// A single line item within a basket, with hydrated product details.
    /// </summary>
    public class BasketItemDto
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public int Quantity { get; set; }
        public DateTime AddedAt { get; set; }
        public ProductDto? Product { get; set; }

        /// <summary>
        /// Subtotal for this line, computed as Quantity * Product.Price.
        /// </summary>
        public decimal Subtotal => Product == null ? 0m : Product.Price * Quantity;
    }
}
