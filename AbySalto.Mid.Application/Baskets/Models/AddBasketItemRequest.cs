namespace AbySalto.Mid.Application.Baskets.Models
{
    /// <summary>
    /// Request body for adding a product to the basket.
    /// </summary>
    public class AddBasketItemRequest
    {
        public int ProductId { get; set; }
        public int Quantity { get; set; } = 1;
    }
}
