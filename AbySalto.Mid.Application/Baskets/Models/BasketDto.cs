namespace AbySalto.Mid.Application.Baskets.Models
{
    /// <summary>
    /// The current user's basket with hydrated line items.
    /// </summary>
    public class BasketDto
    {
        public int? Id { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public List<BasketItemDto> Items { get; set; } = new();

        /// <summary>
        /// Total number of distinct line items in the basket.
        /// </summary>
        public int ItemCount => Items.Count;

        /// <summary>
        /// Sum of all line subtotals.
        /// </summary>
        public decimal Total => Items.Sum(i => i.Subtotal);
    }
}
