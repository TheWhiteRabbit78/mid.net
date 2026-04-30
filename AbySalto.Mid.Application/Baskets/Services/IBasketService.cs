using AbySalto.Mid.Application.Baskets.Models;
using AbySalto.Mid.Application.Common.Models;

namespace AbySalto.Mid.Application.Baskets.Services
{
    /// <summary>
    /// Application service for managing a user's shopping basket.
    /// </summary>
    public interface IBasketService
    {
        /// <summary>
        /// Returns the user's current basket. If the user has no basket yet,
        /// an empty basket DTO is returned (no 404).
        /// </summary>
        Task<ServiceResult<BasketDto>> GetBasketAsync(string userId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Adds a product to the user's basket. The basket is created on first use.
        /// Fails with 409 if the product is already in the basket — use UpdateItem to change quantity.
        /// </summary>
        Task<ServiceResult<BasketItemDto>> AddItemAsync(string userId, AddBasketItemRequest request, CancellationToken cancellationToken = default);

        /// <summary>
        /// Updates the quantity of an existing basket item.
        /// </summary>
        Task<ServiceResult<BasketItemDto>> UpdateItemAsync(string userId, int productId, UpdateBasketItemRequest request, CancellationToken cancellationToken = default);

        /// <summary>
        /// Removes a product from the user's basket.
        /// </summary>
        Task<ServiceResult> RemoveItemAsync(string userId, int productId, CancellationToken cancellationToken = default);
    }
}
