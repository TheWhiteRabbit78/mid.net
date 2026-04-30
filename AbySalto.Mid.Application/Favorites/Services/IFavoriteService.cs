using AbySalto.Mid.Application.Common.Models;
using AbySalto.Mid.Application.Favorites.Models;

namespace AbySalto.Mid.Application.Favorites.Services
{
    /// <summary>
    /// Application service for managing a user's favorited products.
    /// </summary>
    public interface IFavoriteService
    {
        /// <summary>
        /// Returns all favorites for the given user, with product details hydrated from the cache.
        /// </summary>
        Task<ServiceResult<List<FavoriteDto>>> GetUserFavoritesAsync(string userId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Adds a product to the user's favorites. Validates the product exists in DummyJSON
        /// and that the user has not already favorited it.
        /// </summary>
        Task<ServiceResult<FavoriteDto>> AddFavoriteAsync(string userId, int productId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Removes a product from the user's favorites.
        /// </summary>
        Task<ServiceResult> RemoveFavoriteAsync(string userId, int productId, CancellationToken cancellationToken = default);
    }
}
