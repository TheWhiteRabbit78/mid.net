using AbySalto.Mid.Application.Favorites.Models;
using AbySalto.Mid.Application.Products.Models;
using AbySalto.Mid.Domain.Favorites;

namespace AbySalto.Mid.Application.Favorites.Extensions;

public static class FavoriteExtensions
{
    extension(Favorite favorite)
    {
        /// <summary>
        /// Maps a <see cref="Favorite"/> entity to a <see cref="FavoriteDto"/>, optionally
        /// attaching the hydrated product details.
        /// </summary>
        public FavoriteDto MapToDto(ProductDto? product = null)
        {
            return new FavoriteDto
            {
                Id = favorite.Id,
                ProductId = favorite.ProductId,
                CreatedAt = favorite.CreatedAt,
                Product = product,
            };
        }
    }
}
