using AbySalto.Mid.Application.Baskets.Models;
using AbySalto.Mid.Application.Products.Models;
using AbySalto.Mid.Domain.Baskets;

namespace AbySalto.Mid.Application.Baskets.Extensions;

public static class BasketExtensions
{
    extension(BasketItem item)
    {
        /// <summary>
        /// Maps a <see cref="BasketItem"/> entity to a <see cref="BasketItemDto"/>, optionally
        /// attaching the hydrated product details.
        /// </summary>
        public BasketItemDto MapToDto(ProductDto? product = null)
        {
            return new BasketItemDto
            {
                Id = item.Id,
                ProductId = item.ProductId,
                Quantity = item.Quantity,
                AddedAt = item.AddedAt,
                Product = product,
            };
        }
    }

    extension(Basket basket)
    {
        /// <summary>
        /// Maps a <see cref="Basket"/> entity to a <see cref="BasketDto"/> with the supplied
        /// hydrated items.
        /// </summary>
        public BasketDto MapToDto(List<BasketItemDto> items)
        {
            return new BasketDto
            {
                Id = basket.Id,
                CreatedAt = basket.CreatedAt,
                UpdatedAt = basket.UpdatedAt,
                Items = items,
            };
        }
    }
}
