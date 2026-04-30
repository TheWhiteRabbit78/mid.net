using AbySalto.Mid.Application.Products.Models;

namespace AbySalto.Mid.Application.Favorites.Models
{
    /// <summary>
    /// Public representation of a favorited product. The product details are hydrated from
    /// the cached DummyJSON catalogue at read time, so prices and metadata are always current.
    /// </summary>
    public class FavoriteDto
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public DateTime CreatedAt { get; set; }
        public ProductDto? Product { get; set; }
    }
}
