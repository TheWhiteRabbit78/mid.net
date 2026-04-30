namespace AbySalto.Mid.Application.Products.Models
{
    /// <summary>
    /// Strongly-typed configuration for the DummyJSON external API and product caching.
    /// Bound from the "DummyJson" configuration section.
    /// </summary>
    public class DummyJsonSettings
    {
        public const string SectionName = "DummyJson";

        public string BaseUrl { get; set; } = "https://dummyjson.com/";

        /// <summary>
        /// In-memory cache TTL, in minutes, applied to product list and detail lookups.
        /// </summary>
        public int CacheMinutes { get; set; } = 5;
    }
}
