using System.Net;
using AbySalto.Mid.Application.Common.Models;
using AbySalto.Mid.Application.Common.Persistence;
using AbySalto.Mid.Application.Favorites.Extensions;
using AbySalto.Mid.Application.Favorites.Models;
using AbySalto.Mid.Application.Products.Models;
using AbySalto.Mid.Application.Products.Services;
using AbySalto.Mid.Domain.Favorites;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace AbySalto.Mid.Application.Favorites.Services
{
    /// <summary>
    /// Default <see cref="IFavoriteService"/> implementation. Persists favorites in the application
    /// database and hydrates product details on read via the cached <see cref="IProductService"/>.
    /// </summary>
    public class FavoriteService : IFavoriteService
    {
        private readonly IApplicationDbContext _dbContext;
        private readonly IProductService _productService;
        private readonly ILogger<FavoriteService> _logger;

        public FavoriteService(
            IApplicationDbContext dbContext,
            IProductService productService,
            ILogger<FavoriteService> logger)
        {
            _dbContext = dbContext;
            _productService = productService;
            _logger = logger;
        }

        public async Task<ServiceResult<List<FavoriteDto>>> GetUserFavoritesAsync(string userId, CancellationToken cancellationToken = default)
        {
            List<Favorite> favorites = await _dbContext.Favorites
                .Where(f => f.UserId == userId)
                .OrderByDescending(f => f.CreatedAt)
                .ToListAsync(cancellationToken);

            List<FavoriteDto> result = new List<FavoriteDto>(favorites.Count);

            foreach (Favorite favorite in favorites)
            {
                ProductDto? product = await _productService.GetProductByIdAsync(favorite.ProductId, cancellationToken);
                result.Add(favorite.MapToDto(product));
            }

            return ServiceResult<List<FavoriteDto>>.Ok(result);
        }

        public async Task<ServiceResult<FavoriteDto>> AddFavoriteAsync(string userId, int productId, CancellationToken cancellationToken = default)
        {
            ProductDto? product = await _productService.GetProductByIdAsync(productId, cancellationToken);
            if (product == null)
            {
                return ServiceResult<FavoriteDto>.Fail(HttpStatusCode.NotFound, $"Product {productId} does not exist");
            }

            bool alreadyFavorited = await _dbContext.Favorites
                .AnyAsync(f => f.UserId == userId && f.ProductId == productId, cancellationToken);

            if (alreadyFavorited)
            {
                return ServiceResult<FavoriteDto>.Fail(HttpStatusCode.Conflict, "Product is already in favorites");
            }

            Favorite favorite = new Favorite
            {
                UserId = userId,
                ProductId = productId,
                CreatedAt = DateTime.UtcNow,
            };

            _dbContext.Favorites.Add(favorite);

            try
            {
                await _dbContext.SaveChangesAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to add favorite for user {userId}, product {productId}", userId, productId);
                return ServiceResult<FavoriteDto>.Fail(HttpStatusCode.InternalServerError, "Failed to add favorite");
            }

            return ServiceResult<FavoriteDto>.Ok(favorite.MapToDto(product));
        }

        public async Task<ServiceResult> RemoveFavoriteAsync(string userId, int productId, CancellationToken cancellationToken = default)
        {
            Favorite? favorite = await _dbContext.Favorites
                .FirstOrDefaultAsync(f => f.UserId == userId && f.ProductId == productId, cancellationToken);

            if (favorite == null)
            {
                return ServiceResult.Fail(HttpStatusCode.NotFound, "Favorite not found");
            }

            _dbContext.Favorites.Remove(favorite);

            try
            {
                await _dbContext.SaveChangesAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to remove favorite for user {userId}, product {productId}", userId, productId);
                return ServiceResult.Fail(HttpStatusCode.InternalServerError, "Failed to remove favorite");
            }

            return ServiceResult.Ok();
        }
    }
}
