using AbySalto.Mid.Application.Products.Models;

namespace AbySalto.Mid.Application.Products.Services
{
    /// <summary>
    /// Application-level service for product data. Wraps <see cref="IDummyJsonClient"/> with
    /// an in-memory cache to reduce external API calls.
    /// </summary>
    public interface IProductService
    {
        /// <summary>
        /// Returns a paginated, optionally sorted list of products. Results are cached per
        /// (page, pageSize, sortBy, order) tuple.
        /// </summary>
        Task<ProductListDto> GetProductsAsync(ProductQuery query, CancellationToken cancellationToken = default);

        /// <summary>
        /// Returns a single product by id, or null if not found. Results are cached per id.
        /// </summary>
        Task<ProductDto?> GetProductByIdAsync(int id, CancellationToken cancellationToken = default);
    }
}
