namespace AbySalto.Mid.Application.Baskets.Models
{
    /// <summary>
    /// Request body for updating the quantity of an existing basket item.
    /// </summary>
    public class UpdateBasketItemRequest
    {
        public int Quantity { get; set; }
    }
}
