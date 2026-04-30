using System.Security.Claims;
using AbySalto.Mid.Application.Common.Models;
using AbySalto.Mid.Application.Favorites.Models;
using AbySalto.Mid.Application.Favorites.Services;
using AbySalto.Mid.WebApi.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AbySalto.Mid.WebApi.Controllers
{
    /// <summary>
    /// Controller exposing the current user's favorited products.
    /// </summary>
    [ApiController]
    [Authorize]
    [Route("v1/favorites")]
    public class FavoriteController : ControllerBase
    {
        private readonly IFavoriteService _favoriteService;

        public FavoriteController(IFavoriteService favoriteService)
        {
            _favoriteService = favoriteService;
        }

        /// <summary>
        /// Returns the current user's favorites with hydrated product details.
        /// </summary>
        /// <param name="cancellationToken">Request cancellation token.</param>
        /// <returns>The user's favorites.</returns>
        /// <response code="200">Favorites returned successfully.</response>
        /// <response code="401">No valid bearer token supplied.</response>
        [HttpGet]
        [Produces("application/json")]
        [ProducesResponseType<List<FavoriteDto>>(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetFavorites(CancellationToken cancellationToken)
        {
            string? userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return Problem(statusCode: StatusCodes.Status401Unauthorized, title: "Invalid token");
            }

            ServiceResult<List<FavoriteDto>> result = await _favoriteService.GetUserFavoritesAsync(userId, cancellationToken);
            return this.ToActionResult(result);
        }

        /// <summary>
        /// Adds a product to the current user's favorites.
        /// </summary>
        /// <param name="productId">The DummyJSON product id to favorite.</param>
        /// <param name="cancellationToken">Request cancellation token.</param>
        /// <returns>The created favorite.</returns>
        /// <response code="201">Favorite created successfully.</response>
        /// <response code="401">No valid bearer token supplied.</response>
        /// <response code="404">No product with the given id exists.</response>
        /// <response code="409">Product is already in favorites.</response>
        /// <response code="500">Failed to persist the favorite.</response>
        [HttpPost]
        [Route("{productId:int}")]
        [Produces("application/json")]
        [ProducesResponseType<FavoriteDto>(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> AddFavorite([FromRoute] int productId, CancellationToken cancellationToken)
        {
            string? userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return Problem(statusCode: StatusCodes.Status401Unauthorized, title: "Invalid token");
            }

            ServiceResult<FavoriteDto> result = await _favoriteService.AddFavoriteAsync(userId, productId, cancellationToken);
            return this.ToActionResult(result, StatusCodes.Status201Created);
        }

        /// <summary>
        /// Removes a product from the current user's favorites.
        /// </summary>
        /// <param name="productId">The DummyJSON product id to unfavorite.</param>
        /// <param name="cancellationToken">Request cancellation token.</param>
        /// <response code="204">Favorite removed successfully.</response>
        /// <response code="401">No valid bearer token supplied.</response>
        /// <response code="404">Favorite not found.</response>
        /// <response code="500">Failed to remove the favorite.</response>
        [HttpDelete]
        [Route("{productId:int}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> RemoveFavorite([FromRoute] int productId, CancellationToken cancellationToken)
        {
            string? userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return Problem(statusCode: StatusCodes.Status401Unauthorized, title: "Invalid token");
            }

            ServiceResult result = await _favoriteService.RemoveFavoriteAsync(userId, productId, cancellationToken);
            return this.ToActionResult(result);
        }
    }
}
