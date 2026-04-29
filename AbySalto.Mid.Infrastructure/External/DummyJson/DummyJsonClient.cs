using System.Net;
using System.Net.Http.Json;
using AbySalto.Mid.Application.Products.Models;
using AbySalto.Mid.Application.Products.Services;
using AbySalto.Mid.Infrastructure.External.DummyJson.Models;

namespace AbySalto.Mid.Infrastructure.External.DummyJson
{
    /// <summary>
    /// Typed HttpClient implementation of <see cref="IDummyJsonClient"/>. Translates
    /// DummyJSON's raw response shape into application DTOs.
    /// </summary>
    public class DummyJsonClient : IDummyJsonClient
    {
        private readonly HttpClient _httpClient;

        public DummyJsonClient(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<ProductListDto> GetProductsAsync(ProductQuery query, CancellationToken cancellationToken = default)
        {
            int skip = Math.Max(0, (query.Page - 1) * query.PageSize);
            int limit = Math.Max(1, query.PageSize);

            string url = $"products?limit={limit}&skip={skip}";

            if (!string.IsNullOrWhiteSpace(query.SortBy))
            {
                url += $"&sortBy={Uri.EscapeDataString(query.SortBy)}";
                url += $"&order={Uri.EscapeDataString(query.Order ?? "asc")}";
            }

            DummyJsonProductsResponse? response = await _httpClient.GetFromJsonAsync<DummyJsonProductsResponse>(url, cancellationToken);

            if (response == null)
            {
                return new ProductListDto { Page = query.Page, PageSize = query.PageSize };
            }

            return new ProductListDto
            {
                Products = response.Products.Select(MapToDto).ToList(),
                Total = response.Total,
                Page = query.Page,
                PageSize = query.PageSize,
            };
        }

        public async Task<ProductDto?> GetProductByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            HttpResponseMessage response = await _httpClient.GetAsync($"products/{id}", cancellationToken);

            if (response.StatusCode == HttpStatusCode.NotFound)
            {
                return null;
            }

            response.EnsureSuccessStatusCode();

            DummyJsonProduct? product = await response.Content.ReadFromJsonAsync<DummyJsonProduct>(cancellationToken);
            return product == null ? null : MapToDto(product);
        }

        private static ProductDto MapToDto(DummyJsonProduct source)
        {
            return new ProductDto
            {
                Id = source.Id,
                Title = source.Title,
                Description = source.Description,
                Category = source.Category,
                Price = source.Price,
                DiscountPercentage = source.DiscountPercentage,
                Rating = source.Rating,
                Stock = source.Stock,
                Brand = source.Brand,
                Thumbnail = source.Thumbnail,
                Images = source.Images,
            };
        }
    }
}
