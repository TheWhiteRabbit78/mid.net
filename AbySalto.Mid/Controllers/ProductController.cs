using AbySalto.Mid.Application.Products.Models;
using AbySalto.Mid.Application.Products.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AbySalto.Mid.WebApi.Controllers
{
    /// <summary>
    /// Controller exposing the DummyJSON product catalogue with caching, pagination, and sorting.
    /// </summary>
    [ApiController]
    [Authorize]
    [Route("v1/products")]
    public class ProductController : ControllerBase
    {
        private readonly IProductService _productService;

        public ProductController(IProductService productService)
        {
            _productService = productService;
        }

        /// <summary>
        /// Returns a paginated, optionally sorted list of products.
        /// </summary>
        /// <param name="query">Pagination and sort parameters.</param>
        /// <param name="cancellationToken">Request cancellation token.</param>
        /// <returns>A paginated list of products.</returns>
        /// <response code="200">Products returned successfully.</response>
        /// <response code="401">No valid bearer token supplied.</response>
        [HttpGet]
        [Produces("application/json")]
        [ProducesResponseType<ProductListDto>(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetProducts([FromQuery] ProductQuery query, CancellationToken cancellationToken)
        {
            ProductListDto result = await _productService.GetProductsAsync(query, cancellationToken);
            return Ok(result);
        }

        /// <summary>
        /// Returns a single product by its DummyJSON identifier.
        /// </summary>
        /// <param name="id">The product id.</param>
        /// <param name="cancellationToken">Request cancellation token.</param>
        /// <returns>The matching product.</returns>
        /// <response code="200">Product found.</response>
        /// <response code="401">No valid bearer token supplied.</response>
        /// <response code="404">No product with the given id exists.</response>
        [HttpGet]
        [Route("{id:int}")]
        [Produces("application/json")]
        [ProducesResponseType<ProductDto>(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetProduct([FromRoute] int id, CancellationToken cancellationToken)
        {
            ProductDto? product = await _productService.GetProductByIdAsync(id, cancellationToken);

            if (product == null)
            {
                return Problem(statusCode: StatusCodes.Status404NotFound, title: "Product not found");
            }

            return Ok(product);
        }
    }
}
