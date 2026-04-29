namespace AbySalto.Mid.Application.Products.Models
{
    /// <summary>
    /// Query parameters for listing products with pagination and sorting.
    /// </summary>
    public class ProductQuery
    {
        /// <summary>
        /// 1-based page index. Defaults to 1.
        /// </summary>
        public int Page { get; set; } = 1;

        /// <summary>
        /// Number of items per page. Defaults to 30, capped at 100.
        /// </summary>
        public int PageSize { get; set; } = 30;

        /// <summary>
        /// Field to sort by (e.g. "title", "price"). Optional — if null, DummyJSON returns natural order.
        /// </summary>
        public string? SortBy { get; set; }

        /// <summary>
        /// Sort order: "asc" or "desc". Defaults to "asc" when SortBy is provided.
        /// </summary>
        public string? Order { get; set; }
    }
}
