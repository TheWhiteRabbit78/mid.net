using AbySalto.Mid.Application.Products.Models;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;

namespace AbySalto.Mid.Application.Products.Services
{
    /// <summary>
    /// Default <see cref="IProductService"/> implementation backed by <see cref="IMemoryCache"/>.
    /// Cache TTL comes from <see cref="DummyJsonSettings.CacheMinutes"/>.
    /// </summary>
    public class ProductService : IProductService
    {
        private const string ListCacheKeyPrefix = "products:list";
        private const string SingleCacheKeyPrefix = "products:single";

        private readonly IDummyJsonClient _client;
        private readonly IMemoryCache _cache;
        private readonly TimeSpan _cacheTtl;

        public ProductService(IDummyJsonClient client, IMemoryCache cache, IOptions<DummyJsonSettings> settings)
        {
            _client = client;
            _cache = cache;
            _cacheTtl = TimeSpan.FromMinutes(settings.Value.CacheMinutes);
        }

        public async Task<ProductListDto> GetProductsAsync(ProductQuery query, CancellationToken cancellationToken = default)
        {
            string cacheKey = BuildListCacheKey(query);

            if (_cache.TryGetValue(cacheKey, out ProductListDto? cached) && cached != null)
            {
                return cached;
            }

            ProductListDto fresh = await _client.GetProductsAsync(query, cancellationToken);

            _cache.Set(cacheKey, fresh, new MemoryCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = _cacheTtl,
            });

            return fresh;
        }

        public async Task<ProductDto?> GetProductByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            string cacheKey = $"{SingleCacheKeyPrefix}:{id}";

            if (_cache.TryGetValue(cacheKey, out ProductDto? cached) && cached != null)
            {
                return cached;
            }

            ProductDto? fresh = await _client.GetProductByIdAsync(id, cancellationToken);

            if (fresh != null)
            {
                _cache.Set(cacheKey, fresh, new MemoryCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = _cacheTtl,
                });
            }

            return fresh;
        }

        private static string BuildListCacheKey(ProductQuery query)
        {
            return $"{ListCacheKeyPrefix}:{query.Page}:{query.PageSize}:{query.SortBy ?? "_"}:{query.Order ?? "_"}";
        }
    }
}
