using AbySalto.Mid.Application.Products.Models;

namespace AbySalto.Mid.Application.Products.Services
{
    /// <summary>
    /// Adapter for the DummyJSON external API. Returns domain DTOs — DummyJSON's raw schema
    /// is hidden behind the implementation.
    /// </summary>
    public interface IDummyJsonClient
    {
        /// <summary>
        /// Fetches a paginated list of products from DummyJSON.
        /// </summary>
        Task<ProductListDto> GetProductsAsync(ProductQuery query, CancellationToken cancellationToken = default);

        /// <summary>
        /// Fetches a single product by id. Returns null if the product doesn't exist.
        /// </summary>
        Task<ProductDto?> GetProductByIdAsync(int id, CancellationToken cancellationToken = default);
    }
}
