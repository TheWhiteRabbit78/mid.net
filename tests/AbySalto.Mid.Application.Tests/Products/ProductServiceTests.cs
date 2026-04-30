using AbySalto.Mid.Application.Products.Models;
using AbySalto.Mid.Application.Products.Services;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;

namespace AbySalto.Mid.Application.Tests.Products
{
    public class ProductServiceTests
    {
        private readonly IDummyJsonClient _client;
        private readonly IMemoryCache _cache;
        private readonly ProductService _sut;

        public ProductServiceTests()
        {
            _client = Substitute.For<IDummyJsonClient>();
            _cache = new MemoryCache(new MemoryCacheOptions());

            IOptions<DummyJsonSettings> settings = Options.Create(new DummyJsonSettings
            {
                BaseUrl = "https://dummyjson.com/",
                CacheMinutes = 5,
            });

            _sut = new ProductService(_client, _cache, settings);
        }

        #region GetProductsAsync

        [Fact]
        public async Task GetProductsAsync_FirstCall_HitsClient()
        {
            CancellationToken ct = TestContext.Current.CancellationToken;
            ProductQuery query = new ProductQuery { Page = 1, PageSize = 10 };
            _client.GetProductsAsync(Arg.Any<ProductQuery>(), Arg.Any<CancellationToken>())
                   .Returns(BuildProductList(query));

            ProductListDto result = await _sut.GetProductsAsync(query, ct);

            Assert.NotNull(result);
            await _client.Received(1).GetProductsAsync(Arg.Any<ProductQuery>(), Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task GetProductsAsync_SecondCallSameQuery_ReturnsFromCacheWithoutHittingClient()
        {
            CancellationToken ct = TestContext.Current.CancellationToken;
            ProductQuery query = new ProductQuery { Page = 1, PageSize = 10 };
            _client.GetProductsAsync(Arg.Any<ProductQuery>(), Arg.Any<CancellationToken>())
                   .Returns(BuildProductList(query));

            ProductListDto first = await _sut.GetProductsAsync(query, ct);
            ProductListDto second = await _sut.GetProductsAsync(query, ct);

            Assert.Same(first, second);
            await _client.Received(1).GetProductsAsync(Arg.Any<ProductQuery>(), Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task GetProductsAsync_DifferentPage_TriggersFreshClientCall()
        {
            CancellationToken ct = TestContext.Current.CancellationToken;
            _client.GetProductsAsync(Arg.Any<ProductQuery>(), Arg.Any<CancellationToken>())
                   .Returns(callInfo => BuildProductList(callInfo.Arg<ProductQuery>()));

            await _sut.GetProductsAsync(new ProductQuery { Page = 1, PageSize = 10 }, ct);
            await _sut.GetProductsAsync(new ProductQuery { Page = 2, PageSize = 10 }, ct);

            await _client.Received(2).GetProductsAsync(Arg.Any<ProductQuery>(), Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task GetProductsAsync_DifferentSort_TriggersFreshClientCall()
        {
            CancellationToken ct = TestContext.Current.CancellationToken;
            _client.GetProductsAsync(Arg.Any<ProductQuery>(), Arg.Any<CancellationToken>())
                   .Returns(callInfo => BuildProductList(callInfo.Arg<ProductQuery>()));

            await _sut.GetProductsAsync(new ProductQuery { Page = 1, PageSize = 10, SortBy = "title", Order = "asc" }, ct);
            await _sut.GetProductsAsync(new ProductQuery { Page = 1, PageSize = 10, SortBy = "price", Order = "asc" }, ct);

            await _client.Received(2).GetProductsAsync(Arg.Any<ProductQuery>(), Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task GetProductsAsync_DifferentOrder_TriggersFreshClientCall()
        {
            CancellationToken ct = TestContext.Current.CancellationToken;
            _client.GetProductsAsync(Arg.Any<ProductQuery>(), Arg.Any<CancellationToken>())
                   .Returns(callInfo => BuildProductList(callInfo.Arg<ProductQuery>()));

            await _sut.GetProductsAsync(new ProductQuery { Page = 1, PageSize = 10, SortBy = "price", Order = "asc" }, ct);
            await _sut.GetProductsAsync(new ProductQuery { Page = 1, PageSize = 10, SortBy = "price", Order = "desc" }, ct);

            await _client.Received(2).GetProductsAsync(Arg.Any<ProductQuery>(), Arg.Any<CancellationToken>());
        }

        #endregion GetProductsAsync

        #region GetProductByIdAsync

        [Fact]
        public async Task GetProductByIdAsync_FirstCall_HitsClient()
        {
            CancellationToken ct = TestContext.Current.CancellationToken;
            _client.GetProductByIdAsync(Arg.Any<int>(), Arg.Any<CancellationToken>())
                   .Returns(BuildProduct(1));

            ProductDto? result = await _sut.GetProductByIdAsync(1, ct);

            Assert.NotNull(result);
            Assert.Equal(1, result.Id);
            await _client.Received(1).GetProductByIdAsync(1, Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task GetProductByIdAsync_SecondCallSameId_ReturnsFromCacheWithoutHittingClient()
        {
            CancellationToken ct = TestContext.Current.CancellationToken;
            _client.GetProductByIdAsync(Arg.Any<int>(), Arg.Any<CancellationToken>())
                   .Returns(BuildProduct(1));

            ProductDto? first = await _sut.GetProductByIdAsync(1, ct);
            ProductDto? second = await _sut.GetProductByIdAsync(1, ct);

            Assert.Same(first, second);
            await _client.Received(1).GetProductByIdAsync(1, Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task GetProductByIdAsync_DifferentIds_TriggersFreshClientCall()
        {
            CancellationToken ct = TestContext.Current.CancellationToken;
            _client.GetProductByIdAsync(Arg.Any<int>(), Arg.Any<CancellationToken>())
                   .Returns(callInfo => BuildProduct(callInfo.Arg<int>()));

            await _sut.GetProductByIdAsync(1, ct);
            await _sut.GetProductByIdAsync(2, ct);

            await _client.Received(1).GetProductByIdAsync(1, Arg.Any<CancellationToken>());
            await _client.Received(1).GetProductByIdAsync(2, Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task GetProductByIdAsync_NullResult_IsNotCached()
        {
            // A missing product must not be cached — otherwise a product added later
            // to the upstream catalogue would stay "not found" until the cache expires.
            CancellationToken ct = TestContext.Current.CancellationToken;
            _client.GetProductByIdAsync(Arg.Any<int>(), Arg.Any<CancellationToken>())
                   .Returns((ProductDto?)null);

            ProductDto? first = await _sut.GetProductByIdAsync(999, ct);
            ProductDto? second = await _sut.GetProductByIdAsync(999, ct);

            Assert.Null(first);
            Assert.Null(second);
            await _client.Received(2).GetProductByIdAsync(999, Arg.Any<CancellationToken>());
        }

        #endregion GetProductByIdAsync

        #region Helpers

        private static ProductListDto BuildProductList(ProductQuery query)
        {
            return new ProductListDto
            {
                Products = new List<ProductDto> { BuildProduct(1), BuildProduct(2) },
                Total = 2,
                Page = query.Page,
                PageSize = query.PageSize,
            };
        }

        private static ProductDto BuildProduct(int id)
        {
            return new ProductDto
            {
                Id = id,
                Title = $"Product {id}",
                Price = 9.99m * id,
                Stock = 10,
            };
        }

        #endregion Helpers
    }
}
