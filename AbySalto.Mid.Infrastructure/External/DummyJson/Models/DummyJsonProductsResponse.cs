using System.Text.Json.Serialization;

namespace AbySalto.Mid.Infrastructure.External.DummyJson.Models
{
    /// <summary>
    /// Wrapper response shape returned by GET /products on DummyJSON.
    /// </summary>
    internal class DummyJsonProductsResponse
    {
        [JsonPropertyName("products")]
        public List<DummyJsonProduct> Products { get; set; } = new();

        [JsonPropertyName("total")]
        public int Total { get; set; }

        [JsonPropertyName("skip")]
        public int Skip { get; set; }

        [JsonPropertyName("limit")]
        public int Limit { get; set; }
    }
}
