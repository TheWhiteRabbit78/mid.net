using System.Net;
using AbySalto.Mid.Application.Baskets.Extensions;
using AbySalto.Mid.Application.Baskets.Models;
using AbySalto.Mid.Application.Common.Models;
using AbySalto.Mid.Application.Common.Persistence;
using AbySalto.Mid.Application.Products.Models;
using AbySalto.Mid.Application.Products.Services;
using AbySalto.Mid.Domain.Baskets;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace AbySalto.Mid.Application.Baskets.Services
{
    /// <summary>
    /// Default <see cref="IBasketService"/> implementation. Persists baskets and their items
    /// in the application database and hydrates product details on read via
    /// the cached <see cref="IProductService"/>.
    /// </summary>
    public class BasketService : IBasketService
    {
        private readonly IApplicationDbContext _dbContext;
        private readonly IProductService _productService;
        private readonly IValidator<AddBasketItemRequest> _addValidator;
        private readonly IValidator<UpdateBasketItemRequest> _updateValidator;
        private readonly ILogger<BasketService> _logger;

        public BasketService(
            IApplicationDbContext dbContext,
            IProductService productService,
            IValidator<AddBasketItemRequest> addValidator,
            IValidator<UpdateBasketItemRequest> updateValidator,
            ILogger<BasketService> logger)
        {
            _dbContext = dbContext;
            _productService = productService;
            _addValidator = addValidator;
            _updateValidator = updateValidator;
            _logger = logger;
        }

        public async Task<ServiceResult<BasketDto>> GetBasketAsync(string userId, CancellationToken cancellationToken = default)
        {
            Basket? basket = await _dbContext.Baskets
                .Include(b => b.BasketItemsFK)
                .FirstOrDefaultAsync(b => b.UserId == userId, cancellationToken);

            if (basket == null)
            {
                return ServiceResult<BasketDto>.Ok(new BasketDto());
            }

            List<BasketItemDto> hydratedItems = await HydrateItemsAsync(basket.BasketItemsFK ?? new List<BasketItem>(), cancellationToken);

            return ServiceResult<BasketDto>.Ok(basket.MapToDto(hydratedItems));
        }

        public async Task<ServiceResult<BasketItemDto>> AddItemAsync(string userId, AddBasketItemRequest request, CancellationToken cancellationToken = default)
        {
            FluentValidation.Results.ValidationResult validation = await _addValidator.ValidateAsync(request, cancellationToken);
            if (!validation.IsValid)
            {
                return ServiceResult<BasketItemDto>.Fail(HttpStatusCode.BadRequest, validation.Errors.First().ErrorMessage);
            }

            ProductDto? product = await _productService.GetProductByIdAsync(request.ProductId, cancellationToken);
            if (product == null)
            {
                return ServiceResult<BasketItemDto>.Fail(HttpStatusCode.NotFound, $"Product {request.ProductId} does not exist");
            }

            Basket basket = await GetOrCreateBasketAsync(userId, cancellationToken);

            bool alreadyInBasket = await _dbContext.BasketItems
                .AnyAsync(bi => bi.BasketId == basket.Id && bi.ProductId == request.ProductId, cancellationToken);

            if (alreadyInBasket)
            {
                return ServiceResult<BasketItemDto>.Fail(HttpStatusCode.Conflict, "Product is already in basket — use update to change quantity");
            }

            BasketItem item = new BasketItem
            {
                BasketId = basket.Id,
                ProductId = request.ProductId,
                Quantity = request.Quantity,
                AddedAt = DateTime.UtcNow,
            };

            _dbContext.BasketItems.Add(item);
            basket.UpdatedAt = DateTime.UtcNow;

            try
            {
                await _dbContext.SaveChangesAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to add basket item for user {userId}, product {productId}", userId, request.ProductId);
                return ServiceResult<BasketItemDto>.Fail(HttpStatusCode.InternalServerError, "Failed to add basket item");
            }

            return ServiceResult<BasketItemDto>.Ok(item.MapToDto(product));
        }

        public async Task<ServiceResult<BasketItemDto>> UpdateItemAsync(string userId, int productId, UpdateBasketItemRequest request, CancellationToken cancellationToken = default)
        {
            FluentValidation.Results.ValidationResult validation = await _updateValidator.ValidateAsync(request, cancellationToken);
            if (!validation.IsValid)
            {
                return ServiceResult<BasketItemDto>.Fail(HttpStatusCode.BadRequest, validation.Errors.First().ErrorMessage);
            }

            BasketItem? item = await _dbContext.BasketItems
                .Include(bi => bi.BasketFK)
                .FirstOrDefaultAsync(bi => bi.ProductId == productId && bi.BasketFK!.UserId == userId, cancellationToken);

            if (item == null)
            {
                return ServiceResult<BasketItemDto>.Fail(HttpStatusCode.NotFound, "Basket item not found");
            }

            item.Quantity = request.Quantity;
            if (item.BasketFK != null)
            {
                item.BasketFK.UpdatedAt = DateTime.UtcNow;
            }

            try
            {
                await _dbContext.SaveChangesAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to update basket item for user {userId}, product {productId}", userId, productId);
                return ServiceResult<BasketItemDto>.Fail(HttpStatusCode.InternalServerError, "Failed to update basket item");
            }

            ProductDto? product = await _productService.GetProductByIdAsync(productId, cancellationToken);
            return ServiceResult<BasketItemDto>.Ok(item.MapToDto(product));
        }

        public async Task<ServiceResult> RemoveItemAsync(string userId, int productId, CancellationToken cancellationToken = default)
        {
            BasketItem? item = await _dbContext.BasketItems
                .Include(bi => bi.BasketFK)
                .FirstOrDefaultAsync(bi => bi.ProductId == productId && bi.BasketFK!.UserId == userId, cancellationToken);

            if (item == null)
            {
                return ServiceResult.Fail(HttpStatusCode.NotFound, "Basket item not found");
            }

            _dbContext.BasketItems.Remove(item);
            if (item.BasketFK != null)
            {
                item.BasketFK.UpdatedAt = DateTime.UtcNow;
            }

            try
            {
                await _dbContext.SaveChangesAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to remove basket item for user {userId}, product {productId}", userId, productId);
                return ServiceResult.Fail(HttpStatusCode.InternalServerError, "Failed to remove basket item");
            }

            return ServiceResult.Ok();
        }

        private async Task<Basket> GetOrCreateBasketAsync(string userId, CancellationToken cancellationToken)
        {
            Basket? basket = await _dbContext.Baskets.FirstOrDefaultAsync(b => b.UserId == userId, cancellationToken);

            if (basket != null)
            {
                return basket;
            }

            basket = new Basket
            {
                UserId = userId,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
            };

            _dbContext.Baskets.Add(basket);
            await _dbContext.SaveChangesAsync(cancellationToken);

            return basket;
        }

        private async Task<List<BasketItemDto>> HydrateItemsAsync(IEnumerable<BasketItem> items, CancellationToken cancellationToken)
        {
            List<BasketItemDto> hydrated = new List<BasketItemDto>();

            foreach (BasketItem item in items.OrderByDescending(i => i.AddedAt))
            {
                ProductDto? product = await _productService.GetProductByIdAsync(item.ProductId, cancellationToken);
                hydrated.Add(item.MapToDto(product));
            }

            return hydrated;
        }
    }
}
