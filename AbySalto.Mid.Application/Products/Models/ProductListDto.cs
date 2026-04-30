namespace AbySalto.Mid.Application.Products.Models
{
    /// <summary>
    /// Paginated wrapper for a list of products.
    /// </summary>
    public class ProductListDto
    {
        public List<ProductDto> Products { get; set; } = new();

        /// <summary>
        /// Total number of products available across all pages, as reported by DummyJSON.
        /// </summary>
        public int Total { get; set; }

        /// <summary>
        /// Current page number (1-based).
        /// </summary>
        public int Page { get; set; }

        /// <summary>
        /// Number of items per page.
        /// </summary>
        public int PageSize { get; set; }

        /// <summary>
        /// Total number of pages, computed from <see cref="Total"/> and <see cref="PageSize"/>.
        /// </summary>
        public int TotalPages => PageSize > 0 ? (int)Math.Ceiling((double)Total / PageSize) : 0;
    }
}
