using System.Text.Json.Serialization;

namespace AbySalto.Mid.Infrastructure.External.DummyJson.Models
{
    /// <summary>
    /// Raw product representation as returned by the DummyJSON API. Mapped to
    /// <see cref="AbySalto.Mid.Application.Products.Models.ProductDto"/> by the client.
    /// </summary>
    internal class DummyJsonProduct
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("title")]
        public string Title { get; set; } = string.Empty;

        [JsonPropertyName("description")]
        public string? Description { get; set; }

        [JsonPropertyName("category")]
        public string? Category { get; set; }

        [JsonPropertyName("price")]
        public decimal Price { get; set; }

        [JsonPropertyName("discountPercentage")]
        public decimal DiscountPercentage { get; set; }

        [JsonPropertyName("rating")]
        public decimal Rating { get; set; }

        [JsonPropertyName("stock")]
        public int Stock { get; set; }

        [JsonPropertyName("brand")]
        public string? Brand { get; set; }

        [JsonPropertyName("thumbnail")]
        public string? Thumbnail { get; set; }

        [JsonPropertyName("images")]
        public List<string> Images { get; set; } = new();
    }
}
