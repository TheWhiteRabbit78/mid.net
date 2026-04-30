using System.Net;
using AbySalto.Mid.Application.Baskets.Models;
using AbySalto.Mid.Application.Baskets.Services;
using AbySalto.Mid.Application.Baskets.Validators;
using AbySalto.Mid.Application.Common.Models;
using AbySalto.Mid.Application.Products.Models;
using AbySalto.Mid.Application.Products.Services;
using AbySalto.Mid.Application.Tests.Fixtures;
using AbySalto.Mid.Domain.Baskets;
using AbySalto.Mid.Domain.Identity;
using AbySalto.Mid.Infrastructure.Persistence;
using FluentValidation;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace AbySalto.Mid.Application.Tests.Baskets
{
    public class BasketServiceTests : IClassFixture<DatabaseFixture>
    {
        private readonly DatabaseFixture _fixture;

        public BasketServiceTests(DatabaseFixture fixture)
        {
            _fixture = fixture;
        }

        #region GetBasketAsync

        [Fact]
        public async Task GetBasketAsync_NoBasketYet_ReturnsEmptyBasketDtoNotError()
        {
            CancellationToken ct = TestContext.Current.CancellationToken;
            ApplicationDbContext context = _fixture.CreateContext();
            ApplicationUser user = await DatabaseFixture.SeedUserAsync(context);
            IProductService productService = Substitute.For<IProductService>();
            BasketService sut = CreateSut(context, productService);

            ServiceResult<BasketDto> result = await sut.GetBasketAsync(user.Id, ct);

            Assert.True(result.Success);
            Assert.NotNull(result.Data);
            Assert.Null(result.Data.Id);
            Assert.Empty(result.Data.Items);
            Assert.Equal(0, result.Data.ItemCount);
            Assert.Equal(0m, result.Data.Total);
        }

        [Fact]
        public async Task GetBasketAsync_BasketWithItems_HydratesAndComputesTotal()
        {
            CancellationToken ct = TestContext.Current.CancellationToken;
            ApplicationDbContext context = _fixture.CreateContext();
            ApplicationUser user = await DatabaseFixture.SeedUserAsync(context);

            Basket basket = new Basket { UserId = user.Id, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow };
            basket.BasketItemsFK = new List<BasketItem>
            {
                new BasketItem { ProductId = 1, Quantity = 2, AddedAt = DateTime.UtcNow.AddMinutes(-10) },
                new BasketItem { ProductId = 2, Quantity = 3, AddedAt = DateTime.UtcNow },
            };
            context.Baskets.Add(basket);
            await context.SaveChangesAsync(ct);

            IProductService productService = Substitute.For<IProductService>();
            productService.GetProductByIdAsync(1, Arg.Any<CancellationToken>()).Returns(BuildProduct(1, 10m));
            productService.GetProductByIdAsync(2, Arg.Any<CancellationToken>()).Returns(BuildProduct(2, 5m));

            BasketService sut = CreateSut(context, productService);

            ServiceResult<BasketDto> result = await sut.GetBasketAsync(user.Id, ct);

            Assert.True(result.Success);
            Assert.NotNull(result.Data);
            Assert.Equal(2, result.Data.Items.Count);
            // 2 * 10 + 3 * 5 = 35
            Assert.Equal(35m, result.Data.Total);
        }

        [Fact]
        public async Task GetBasketAsync_BasketWithItems_ReturnsItemsOrderedByAddedAtDescending()
        {
            CancellationToken ct = TestContext.Current.CancellationToken;
            ApplicationDbContext context = _fixture.CreateContext();
            ApplicationUser user = await DatabaseFixture.SeedUserAsync(context);

            Basket basket = new Basket { UserId = user.Id, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow };
            basket.BasketItemsFK = new List<BasketItem>
            {
                new BasketItem { ProductId = 1, Quantity = 1, AddedAt = DateTime.UtcNow.AddHours(-2) },
                new BasketItem { ProductId = 2, Quantity = 1, AddedAt = DateTime.UtcNow.AddHours(-1) },
                new BasketItem { ProductId = 3, Quantity = 1, AddedAt = DateTime.UtcNow },
            };
            context.Baskets.Add(basket);
            await context.SaveChangesAsync(ct);

            IProductService productService = Substitute.For<IProductService>();
            productService.GetProductByIdAsync(Arg.Any<int>(), Arg.Any<CancellationToken>())
                          .Returns(callInfo => BuildProduct(callInfo.Arg<int>(), 1m));

            BasketService sut = CreateSut(context, productService);

            ServiceResult<BasketDto> result = await sut.GetBasketAsync(user.Id, ct);

            Assert.True(result.Success);
            Assert.NotNull(result.Data);
            Assert.Equal(3, result.Data.Items[0].ProductId);
            Assert.Equal(2, result.Data.Items[1].ProductId);
            Assert.Equal(1, result.Data.Items[2].ProductId);
        }

        #endregion GetBasketAsync

        #region AddItemAsync

        [Fact]
        public async Task AddItemAsync_InvalidRequest_ReturnsBadRequest()
        {
            CancellationToken ct = TestContext.Current.CancellationToken;
            ApplicationDbContext context = _fixture.CreateContext();
            ApplicationUser user = await DatabaseFixture.SeedUserAsync(context);

            IProductService productService = Substitute.For<IProductService>();
            BasketService sut = CreateSut(context, productService);

            ServiceResult<BasketItemDto> result = await sut.AddItemAsync(user.Id, new AddBasketItemRequest { ProductId = 1, Quantity = 0 }, ct);

            Assert.False(result.Success);
            Assert.Equal(HttpStatusCode.BadRequest, result.Code);
        }

        [Fact]
        public async Task AddItemAsync_ProductDoesNotExist_ReturnsNotFound()
        {
            CancellationToken ct = TestContext.Current.CancellationToken;
            ApplicationDbContext context = _fixture.CreateContext();
            ApplicationUser user = await DatabaseFixture.SeedUserAsync(context);

            IProductService productService = Substitute.For<IProductService>();
            productService.GetProductByIdAsync(Arg.Any<int>(), Arg.Any<CancellationToken>())
                          .Returns((ProductDto?)null);

            BasketService sut = CreateSut(context, productService);

            ServiceResult<BasketItemDto> result = await sut.AddItemAsync(user.Id, new AddBasketItemRequest { ProductId = 999, Quantity = 1 }, ct);

            Assert.False(result.Success);
            Assert.Equal(HttpStatusCode.NotFound, result.Code);
            Assert.False(await context.BasketItems.AnyAsync(ct));
        }

        [Fact]
        public async Task AddItemAsync_NoBasketYet_CreatesBasketAndAddsItem()
        {
            CancellationToken ct = TestContext.Current.CancellationToken;
            ApplicationDbContext context = _fixture.CreateContext();
            ApplicationUser user = await DatabaseFixture.SeedUserAsync(context);

            IProductService productService = Substitute.For<IProductService>();
            productService.GetProductByIdAsync(7, Arg.Any<CancellationToken>())
                          .Returns(BuildProduct(7, 9.99m));

            BasketService sut = CreateSut(context, productService);

            ServiceResult<BasketItemDto> result = await sut.AddItemAsync(user.Id, new AddBasketItemRequest { ProductId = 7, Quantity = 2 }, ct);

            Assert.True(result.Success);
            Assert.NotNull(result.Data);
            Assert.Equal(7, result.Data.ProductId);
            Assert.Equal(2, result.Data.Quantity);

            Basket? basket = await context.Baskets.FirstOrDefaultAsync(b => b.UserId == user.Id, ct);
            Assert.NotNull(basket);
            Assert.Equal(1, await context.BasketItems.CountAsync(bi => bi.BasketId == basket.Id, ct));
        }

        [Fact]
        public async Task AddItemAsync_ExistingBasket_AddsItemWithoutCreatingNewBasket()
        {
            CancellationToken ct = TestContext.Current.CancellationToken;
            ApplicationDbContext context = _fixture.CreateContext();
            ApplicationUser user = await DatabaseFixture.SeedUserAsync(context);

            Basket existing = new Basket { UserId = user.Id, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow };
            context.Baskets.Add(existing);
            await context.SaveChangesAsync(ct);

            IProductService productService = Substitute.For<IProductService>();
            productService.GetProductByIdAsync(1, Arg.Any<CancellationToken>())
                          .Returns(BuildProduct(1, 5m));

            BasketService sut = CreateSut(context, productService);

            ServiceResult<BasketItemDto> result = await sut.AddItemAsync(user.Id, new AddBasketItemRequest { ProductId = 1, Quantity = 1 }, ct);

            Assert.True(result.Success);
            Assert.Equal(1, await context.Baskets.CountAsync(b => b.UserId == user.Id, ct));
        }

        [Fact]
        public async Task AddItemAsync_ProductAlreadyInBasket_ReturnsConflict()
        {
            CancellationToken ct = TestContext.Current.CancellationToken;
            ApplicationDbContext context = _fixture.CreateContext();
            ApplicationUser user = await DatabaseFixture.SeedUserAsync(context);

            Basket basket = new Basket { UserId = user.Id, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow };
            basket.BasketItemsFK = new List<BasketItem>
            {
                new BasketItem { ProductId = 1, Quantity = 1, AddedAt = DateTime.UtcNow },
            };
            context.Baskets.Add(basket);
            await context.SaveChangesAsync(ct);

            IProductService productService = Substitute.For<IProductService>();
            productService.GetProductByIdAsync(1, Arg.Any<CancellationToken>())
                          .Returns(BuildProduct(1, 5m));

            BasketService sut = CreateSut(context, productService);

            ServiceResult<BasketItemDto> result = await sut.AddItemAsync(user.Id, new AddBasketItemRequest { ProductId = 1, Quantity = 5 }, ct);

            Assert.False(result.Success);
            Assert.Equal(HttpStatusCode.Conflict, result.Code);
            // Existing item must remain untouched (still quantity 1, not overwritten with 5)
            BasketItem? item = await context.BasketItems.FirstOrDefaultAsync(bi => bi.ProductId == 1, ct);
            Assert.NotNull(item);
            Assert.Equal(1, item.Quantity);
        }

        #endregion AddItemAsync

        #region UpdateItemAsync

        [Fact]
        public async Task UpdateItemAsync_InvalidQuantity_ReturnsBadRequest()
        {
            CancellationToken ct = TestContext.Current.CancellationToken;
            ApplicationDbContext context = _fixture.CreateContext();
            ApplicationUser user = await DatabaseFixture.SeedUserAsync(context);

            IProductService productService = Substitute.For<IProductService>();
            BasketService sut = CreateSut(context, productService);

            ServiceResult<BasketItemDto> result = await sut.UpdateItemAsync(user.Id, 1, new UpdateBasketItemRequest { Quantity = 0 }, ct);

            Assert.False(result.Success);
            Assert.Equal(HttpStatusCode.BadRequest, result.Code);
        }

        [Fact]
        public async Task UpdateItemAsync_ItemNotInBasket_ReturnsNotFound()
        {
            CancellationToken ct = TestContext.Current.CancellationToken;
            ApplicationDbContext context = _fixture.CreateContext();
            ApplicationUser user = await DatabaseFixture.SeedUserAsync(context);

            IProductService productService = Substitute.For<IProductService>();
            BasketService sut = CreateSut(context, productService);

            ServiceResult<BasketItemDto> result = await sut.UpdateItemAsync(user.Id, 1, new UpdateBasketItemRequest { Quantity = 5 }, ct);

            Assert.False(result.Success);
            Assert.Equal(HttpStatusCode.NotFound, result.Code);
        }

        [Fact]
        public async Task UpdateItemAsync_ExistingItem_UpdatesQuantityAndReturnsHydratedDto()
        {
            CancellationToken ct = TestContext.Current.CancellationToken;
            ApplicationDbContext context = _fixture.CreateContext();
            ApplicationUser user = await DatabaseFixture.SeedUserAsync(context);

            Basket basket = new Basket { UserId = user.Id, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow };
            basket.BasketItemsFK = new List<BasketItem>
            {
                new BasketItem { ProductId = 1, Quantity = 1, AddedAt = DateTime.UtcNow },
            };
            context.Baskets.Add(basket);
            await context.SaveChangesAsync(ct);

            IProductService productService = Substitute.For<IProductService>();
            productService.GetProductByIdAsync(1, Arg.Any<CancellationToken>())
                          .Returns(BuildProduct(1, 5m));

            BasketService sut = CreateSut(context, productService);

            ServiceResult<BasketItemDto> result = await sut.UpdateItemAsync(user.Id, 1, new UpdateBasketItemRequest { Quantity = 7 }, ct);

            Assert.True(result.Success);
            Assert.NotNull(result.Data);
            Assert.Equal(7, result.Data.Quantity);
            Assert.NotNull(result.Data.Product);

            BasketItem? persisted = await context.BasketItems.FirstOrDefaultAsync(bi => bi.ProductId == 1, ct);
            Assert.NotNull(persisted);
            Assert.Equal(7, persisted.Quantity);
        }

        [Fact]
        public async Task UpdateItemAsync_DoesNotAffectAnotherUsersIdenticalProductLine()
        {
            CancellationToken ct = TestContext.Current.CancellationToken;
            ApplicationDbContext context = _fixture.CreateContext();
            ApplicationUser userA = await DatabaseFixture.SeedUserAsync(context, "userA");
            ApplicationUser userB = await DatabaseFixture.SeedUserAsync(context, "userB");

            Basket basketA = new Basket { UserId = userA.Id, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow };
            basketA.BasketItemsFK = new List<BasketItem>
            {
                new BasketItem { ProductId = 1, Quantity = 1, AddedAt = DateTime.UtcNow },
            };

            Basket basketB = new Basket { UserId = userB.Id, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow };
            basketB.BasketItemsFK = new List<BasketItem>
            {
                new BasketItem { ProductId = 1, Quantity = 1, AddedAt = DateTime.UtcNow },
            };

            context.Baskets.Add(basketA);
            context.Baskets.Add(basketB);
            await context.SaveChangesAsync(ct);

            IProductService productService = Substitute.For<IProductService>();
            productService.GetProductByIdAsync(1, Arg.Any<CancellationToken>())
                          .Returns(BuildProduct(1, 5m));

            BasketService sut = CreateSut(context, productService);

            await sut.UpdateItemAsync(userA.Id, 1, new UpdateBasketItemRequest { Quantity = 99 }, ct);

            BasketItem? itemB = await context.BasketItems
                .Include(bi => bi.BasketFK)
                .FirstOrDefaultAsync(bi => bi.BasketFK!.UserId == userB.Id && bi.ProductId == 1, ct);

            Assert.NotNull(itemB);
            Assert.Equal(1, itemB.Quantity);
        }

        #endregion UpdateItemAsync

        #region RemoveItemAsync

        [Fact]
        public async Task RemoveItemAsync_ItemNotInBasket_ReturnsNotFound()
        {
            CancellationToken ct = TestContext.Current.CancellationToken;
            ApplicationDbContext context = _fixture.CreateContext();
            ApplicationUser user = await DatabaseFixture.SeedUserAsync(context);

            IProductService productService = Substitute.For<IProductService>();
            BasketService sut = CreateSut(context, productService);

            ServiceResult result = await sut.RemoveItemAsync(user.Id, 1, ct);

            Assert.False(result.Success);
            Assert.Equal(HttpStatusCode.NotFound, result.Code);
        }

        [Fact]
        public async Task RemoveItemAsync_ExistingItem_RemovesAndReturnsOk()
        {
            CancellationToken ct = TestContext.Current.CancellationToken;
            ApplicationDbContext context = _fixture.CreateContext();
            ApplicationUser user = await DatabaseFixture.SeedUserAsync(context);

            Basket basket = new Basket { UserId = user.Id, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow };
            basket.BasketItemsFK = new List<BasketItem>
            {
                new BasketItem { ProductId = 1, Quantity = 1, AddedAt = DateTime.UtcNow },
                new BasketItem { ProductId = 2, Quantity = 3, AddedAt = DateTime.UtcNow },
            };
            context.Baskets.Add(basket);
            await context.SaveChangesAsync(ct);

            IProductService productService = Substitute.For<IProductService>();
            BasketService sut = CreateSut(context, productService);

            ServiceResult result = await sut.RemoveItemAsync(user.Id, 1, ct);

            Assert.True(result.Success);
            Assert.False(await context.BasketItems.AnyAsync(bi => bi.ProductId == 1, ct));
            Assert.True(await context.BasketItems.AnyAsync(bi => bi.ProductId == 2, ct));
        }

        [Fact]
        public async Task RemoveItemAsync_DoesNotAffectAnotherUsersIdenticalProductLine()
        {
            CancellationToken ct = TestContext.Current.CancellationToken;
            ApplicationDbContext context = _fixture.CreateContext();
            ApplicationUser userA = await DatabaseFixture.SeedUserAsync(context, "userA");
            ApplicationUser userB = await DatabaseFixture.SeedUserAsync(context, "userB");

            Basket basketA = new Basket { UserId = userA.Id, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow };
            basketA.BasketItemsFK = new List<BasketItem>
            {
                new BasketItem { ProductId = 1, Quantity = 1, AddedAt = DateTime.UtcNow },
            };

            Basket basketB = new Basket { UserId = userB.Id, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow };
            basketB.BasketItemsFK = new List<BasketItem>
            {
                new BasketItem { ProductId = 1, Quantity = 1, AddedAt = DateTime.UtcNow },
            };

            context.Baskets.Add(basketA);
            context.Baskets.Add(basketB);
            await context.SaveChangesAsync(ct);

            IProductService productService = Substitute.For<IProductService>();
            BasketService sut = CreateSut(context, productService);

            await sut.RemoveItemAsync(userA.Id, 1, ct);

            Assert.True(await context.BasketItems
                .Include(bi => bi.BasketFK)
                .AnyAsync(bi => bi.BasketFK!.UserId == userB.Id && bi.ProductId == 1, ct));
        }

        #endregion RemoveItemAsync

        #region Helpers

        private static BasketService CreateSut(ApplicationDbContext context, IProductService productService)
        {
            IValidator<AddBasketItemRequest> addValidator = new AddBasketItemRequestValidator();
            IValidator<UpdateBasketItemRequest> updateValidator = new UpdateBasketItemRequestValidator();

            return new BasketService(
                context,
                productService,
                addValidator,
                updateValidator,
                NullLogger<BasketService>.Instance);
        }

        private static ProductDto BuildProduct(int id, decimal price)
        {
            return new ProductDto
            {
                Id = id,
                Title = $"Product {id}",
                Price = price,
                Stock = 10,
            };
        }

        #endregion Helpers
    }
}
