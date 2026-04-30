using System.Security.Claims;
using AbySalto.Mid.Application.Baskets.Models;
using AbySalto.Mid.Application.Baskets.Services;
using AbySalto.Mid.Application.Common.Models;
using AbySalto.Mid.WebApi.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AbySalto.Mid.WebApi.Controllers
{
    /// <summary>
    /// Controller exposing the current user's shopping basket.
    /// </summary>
    [ApiController]
    [Authorize]
    [Route("v1/basket")]
    public class BasketController : ControllerBase
    {
        private readonly IBasketService _basketService;

        public BasketController(IBasketService basketService)
        {
            _basketService = basketService;
        }

        /// <summary>
        /// Returns the current user's basket with hydrated product details.
        /// </summary>
        /// <param name="cancellationToken">Request cancellation token.</param>
        /// <returns>The user's basket.</returns>
        /// <response code="200">Basket returned successfully. Empty basket is returned if the user has no items yet.</response>
        /// <response code="401">No valid bearer token supplied.</response>
        [HttpGet]
        [Produces("application/json")]
        [ProducesResponseType<BasketDto>(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetBasket(CancellationToken cancellationToken)
        {
            string? userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return Problem(statusCode: StatusCodes.Status401Unauthorized, title: "Invalid token");
            }

            ServiceResult<BasketDto> result = await _basketService.GetBasketAsync(userId, cancellationToken);
            return this.ToActionResult(result);
        }

        /// <summary>
        /// Adds a product to the current user's basket. The basket is created on first use.
        /// </summary>
        /// <param name="request">Product id and quantity to add.</param>
        /// <param name="cancellationToken">Request cancellation token.</param>
        /// <returns>The created basket item.</returns>
        /// <response code="201">Item added successfully.</response>
        /// <response code="400">Validation failure.</response>
        /// <response code="401">No valid bearer token supplied.</response>
        /// <response code="404">No product with the given id exists.</response>
        /// <response code="409">Product is already in the basket — use update to change quantity.</response>
        /// <response code="500">Failed to persist the basket item.</response>
        [HttpPost]
        [Route("items")]
        [Produces("application/json")]
        [ProducesResponseType<BasketItemDto>(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> AddItem([FromBody] AddBasketItemRequest request, CancellationToken cancellationToken)
        {
            string? userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return Problem(statusCode: StatusCodes.Status401Unauthorized, title: "Invalid token");
            }

            ServiceResult<BasketItemDto> result = await _basketService.AddItemAsync(userId, request, cancellationToken);
            return this.ToActionResult(result, StatusCodes.Status201Created);
        }

        /// <summary>
        /// Updates the quantity of an existing basket item.
        /// </summary>
        /// <param name="productId">The product id of the basket item to update.</param>
        /// <param name="request">The new quantity.</param>
        /// <param name="cancellationToken">Request cancellation token.</param>
        /// <returns>The updated basket item.</returns>
        /// <response code="200">Item updated successfully.</response>
        /// <response code="400">Validation failure.</response>
        /// <response code="401">No valid bearer token supplied.</response>
        /// <response code="404">Basket item not found.</response>
        /// <response code="500">Failed to persist the change.</response>
        [HttpPut]
        [Route("items/{productId:int}")]
        [Produces("application/json")]
        [ProducesResponseType<BasketItemDto>(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> UpdateItem([FromRoute] int productId, [FromBody] UpdateBasketItemRequest request, CancellationToken cancellationToken)
        {
            string? userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return Problem(statusCode: StatusCodes.Status401Unauthorized, title: "Invalid token");
            }

            ServiceResult<BasketItemDto> result = await _basketService.UpdateItemAsync(userId, productId, request, cancellationToken);
            return this.ToActionResult(result);
        }

        /// <summary>
        /// Removes a product from the current user's basket.
        /// </summary>
        /// <param name="productId">The product id of the basket item to remove.</param>
        /// <param name="cancellationToken">Request cancellation token.</param>
        /// <response code="204">Item removed successfully.</response>
        /// <response code="401">No valid bearer token supplied.</response>
        /// <response code="404">Basket item not found.</response>
        /// <response code="500">Failed to remove the item.</response>
        [HttpDelete]
        [Route("items/{productId:int}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> RemoveItem([FromRoute] int productId, CancellationToken cancellationToken)
        {
            string? userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return Problem(statusCode: StatusCodes.Status401Unauthorized, title: "Invalid token");
            }

            ServiceResult result = await _basketService.RemoveItemAsync(userId, productId, cancellationToken);
            return this.ToActionResult(result);
        }
    }
}
