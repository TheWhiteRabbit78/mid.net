using System.Net;
using AbySalto.Mid.Application.Common.Models;
using AbySalto.Mid.Application.Favorites.Models;
using AbySalto.Mid.Application.Favorites.Services;
using AbySalto.Mid.Application.Products.Models;
using AbySalto.Mid.Application.Products.Services;
using AbySalto.Mid.Application.Tests.Fixtures;
using AbySalto.Mid.Domain.Favorites;
using AbySalto.Mid.Domain.Identity;
using AbySalto.Mid.Infrastructure.Persistence;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace AbySalto.Mid.Application.Tests.Favorites
{
    public class FavoriteServiceTests : IClassFixture<DatabaseFixture>
    {
        private readonly DatabaseFixture _fixture;

        public FavoriteServiceTests(DatabaseFixture fixture)
        {
            _fixture = fixture;
        }

        #region GetUserFavoritesAsync

        [Fact]
        public async Task GetUserFavoritesAsync_NoFavorites_ReturnsEmptyList()
        {
            CancellationToken ct = TestContext.Current.CancellationToken;
            ApplicationDbContext context = _fixture.CreateContext();
            ApplicationUser user = await DatabaseFixture.SeedUserAsync(context);
            IProductService productService = Substitute.For<IProductService>();
            FavoriteService sut = CreateSut(context, productService);

            ServiceResult<List<FavoriteDto>> result = await sut.GetUserFavoritesAsync(user.Id, ct);

            Assert.True(result.Success);
            Assert.NotNull(result.Data);
            Assert.Empty(result.Data);
        }

        [Fact]
        public async Task GetUserFavoritesAsync_OnlyReturnsFavoritesForGivenUser()
        {
            CancellationToken ct = TestContext.Current.CancellationToken;
            ApplicationDbContext context = _fixture.CreateContext();
            ApplicationUser userA = await DatabaseFixture.SeedUserAsync(context, "userA");
            ApplicationUser userB = await DatabaseFixture.SeedUserAsync(context, "userB");

            context.Favorites.Add(new Favorite { UserId = userA.Id, ProductId = 1, CreatedAt = DateTime.UtcNow });
            context.Favorites.Add(new Favorite { UserId = userB.Id, ProductId = 2, CreatedAt = DateTime.UtcNow });
            await context.SaveChangesAsync(ct);

            IProductService productService = Substitute.For<IProductService>();
            productService.GetProductByIdAsync(Arg.Any<int>(), Arg.Any<CancellationToken>())
                          .Returns(callInfo => BuildProduct(callInfo.Arg<int>()));

            FavoriteService sut = CreateSut(context, productService);

            ServiceResult<List<FavoriteDto>> result = await sut.GetUserFavoritesAsync(userA.Id, ct);

            Assert.True(result.Success);
            Assert.NotNull(result.Data);
            Assert.Single(result.Data);
            Assert.Equal(1, result.Data[0].ProductId);
        }

        [Fact]
        public async Task GetUserFavoritesAsync_ReturnsFavoritesOrderedByCreatedAtDescending()
        {
            CancellationToken ct = TestContext.Current.CancellationToken;
            ApplicationDbContext context = _fixture.CreateContext();
            ApplicationUser user = await DatabaseFixture.SeedUserAsync(context);

            context.Favorites.Add(new Favorite { UserId = user.Id, ProductId = 1, CreatedAt = DateTime.UtcNow.AddHours(-2) });
            context.Favorites.Add(new Favorite { UserId = user.Id, ProductId = 2, CreatedAt = DateTime.UtcNow.AddHours(-1) });
            context.Favorites.Add(new Favorite { UserId = user.Id, ProductId = 3, CreatedAt = DateTime.UtcNow });
            await context.SaveChangesAsync(ct);

            IProductService productService = Substitute.For<IProductService>();
            productService.GetProductByIdAsync(Arg.Any<int>(), Arg.Any<CancellationToken>())
                          .Returns(callInfo => BuildProduct(callInfo.Arg<int>()));

            FavoriteService sut = CreateSut(context, productService);

            ServiceResult<List<FavoriteDto>> result = await sut.GetUserFavoritesAsync(user.Id, ct);

            Assert.True(result.Success);
            Assert.NotNull(result.Data);
            Assert.Equal(3, result.Data.Count);
            Assert.Equal(3, result.Data[0].ProductId);
            Assert.Equal(2, result.Data[1].ProductId);
            Assert.Equal(1, result.Data[2].ProductId);
        }

        [Fact]
        public async Task GetUserFavoritesAsync_HydratesEachFavoriteWithProductDetails()
        {
            CancellationToken ct = TestContext.Current.CancellationToken;
            ApplicationDbContext context = _fixture.CreateContext();
            ApplicationUser user = await DatabaseFixture.SeedUserAsync(context);

            context.Favorites.Add(new Favorite { UserId = user.Id, ProductId = 42, CreatedAt = DateTime.UtcNow });
            await context.SaveChangesAsync(ct);

            IProductService productService = Substitute.For<IProductService>();
            productService.GetProductByIdAsync(42, Arg.Any<CancellationToken>())
                          .Returns(BuildProduct(42));

            FavoriteService sut = CreateSut(context, productService);

            ServiceResult<List<FavoriteDto>> result = await sut.GetUserFavoritesAsync(user.Id, ct);

            Assert.True(result.Success);
            Assert.NotNull(result.Data);
            FavoriteDto favorite = Assert.Single(result.Data);
            Assert.NotNull(favorite.Product);
            Assert.Equal(42, favorite.Product.Id);
            Assert.Equal("Product 42", favorite.Product.Title);
        }

        #endregion GetUserFavoritesAsync

        #region AddFavoriteAsync

        [Fact]
        public async Task AddFavoriteAsync_ProductDoesNotExist_ReturnsNotFound()
        {
            CancellationToken ct = TestContext.Current.CancellationToken;
            ApplicationDbContext context = _fixture.CreateContext();
            ApplicationUser user = await DatabaseFixture.SeedUserAsync(context);

            IProductService productService = Substitute.For<IProductService>();
            productService.GetProductByIdAsync(Arg.Any<int>(), Arg.Any<CancellationToken>())
                          .Returns((ProductDto?)null);

            FavoriteService sut = CreateSut(context, productService);

            ServiceResult<FavoriteDto> result = await sut.AddFavoriteAsync(user.Id, 999, ct);

            Assert.False(result.Success);
            Assert.Equal(HttpStatusCode.NotFound, result.Code);
            Assert.False(await context.Favorites.AnyAsync(ct));
        }

        [Fact]
        public async Task AddFavoriteAsync_ProductAlreadyFavorited_ReturnsConflict()
        {
            CancellationToken ct = TestContext.Current.CancellationToken;
            ApplicationDbContext context = _fixture.CreateContext();
            ApplicationUser user = await DatabaseFixture.SeedUserAsync(context);

            context.Favorites.Add(new Favorite { UserId = user.Id, ProductId = 1, CreatedAt = DateTime.UtcNow });
            await context.SaveChangesAsync(ct);

            IProductService productService = Substitute.For<IProductService>();
            productService.GetProductByIdAsync(1, Arg.Any<CancellationToken>())
                          .Returns(BuildProduct(1));

            FavoriteService sut = CreateSut(context, productService);

            ServiceResult<FavoriteDto> result = await sut.AddFavoriteAsync(user.Id, 1, ct);

            Assert.False(result.Success);
            Assert.Equal(HttpStatusCode.Conflict, result.Code);
            Assert.Equal(1, await context.Favorites.CountAsync(ct));
        }

        [Fact]
        public async Task AddFavoriteAsync_ValidProduct_AddsFavoriteAndReturnsHydratedDto()
        {
            CancellationToken ct = TestContext.Current.CancellationToken;
            ApplicationDbContext context = _fixture.CreateContext();
            ApplicationUser user = await DatabaseFixture.SeedUserAsync(context);

            IProductService productService = Substitute.For<IProductService>();
            productService.GetProductByIdAsync(7, Arg.Any<CancellationToken>())
                          .Returns(BuildProduct(7));

            FavoriteService sut = CreateSut(context, productService);

            ServiceResult<FavoriteDto> result = await sut.AddFavoriteAsync(user.Id, 7, ct);

            Assert.True(result.Success);
            Assert.NotNull(result.Data);
            Assert.Equal(7, result.Data.ProductId);
            Assert.NotNull(result.Data.Product);
            Assert.Equal(7, result.Data.Product.Id);

            Favorite? persisted = await context.Favorites.FirstOrDefaultAsync(f => f.UserId == user.Id && f.ProductId == 7, ct);
            Assert.NotNull(persisted);
        }

        #endregion AddFavoriteAsync

        #region RemoveFavoriteAsync

        [Fact]
        public async Task RemoveFavoriteAsync_FavoriteDoesNotExist_ReturnsNotFound()
        {
            CancellationToken ct = TestContext.Current.CancellationToken;
            ApplicationDbContext context = _fixture.CreateContext();
            ApplicationUser user = await DatabaseFixture.SeedUserAsync(context);

            IProductService productService = Substitute.For<IProductService>();
            FavoriteService sut = CreateSut(context, productService);

            ServiceResult result = await sut.RemoveFavoriteAsync(user.Id, 1, ct);

            Assert.False(result.Success);
            Assert.Equal(HttpStatusCode.NotFound, result.Code);
        }

        [Fact]
        public async Task RemoveFavoriteAsync_ExistingFavorite_RemovesAndReturnsOk()
        {
            CancellationToken ct = TestContext.Current.CancellationToken;
            ApplicationDbContext context = _fixture.CreateContext();
            ApplicationUser user = await DatabaseFixture.SeedUserAsync(context);

            context.Favorites.Add(new Favorite { UserId = user.Id, ProductId = 1, CreatedAt = DateTime.UtcNow });
            await context.SaveChangesAsync(ct);

            IProductService productService = Substitute.For<IProductService>();
            FavoriteService sut = CreateSut(context, productService);

            ServiceResult result = await sut.RemoveFavoriteAsync(user.Id, 1, ct);

            Assert.True(result.Success);
            Assert.False(await context.Favorites.AnyAsync(f => f.UserId == user.Id && f.ProductId == 1, ct));
        }

        [Fact]
        public async Task RemoveFavoriteAsync_DoesNotAffectOtherUsersFavorites()
        {
            CancellationToken ct = TestContext.Current.CancellationToken;
            ApplicationDbContext context = _fixture.CreateContext();
            ApplicationUser userA = await DatabaseFixture.SeedUserAsync(context, "userA");
            ApplicationUser userB = await DatabaseFixture.SeedUserAsync(context, "userB");

            context.Favorites.Add(new Favorite { UserId = userA.Id, ProductId = 1, CreatedAt = DateTime.UtcNow });
            context.Favorites.Add(new Favorite { UserId = userB.Id, ProductId = 1, CreatedAt = DateTime.UtcNow });
            await context.SaveChangesAsync(ct);

            IProductService productService = Substitute.For<IProductService>();
            FavoriteService sut = CreateSut(context, productService);

            ServiceResult result = await sut.RemoveFavoriteAsync(userA.Id, 1, ct);

            Assert.True(result.Success);
            Assert.False(await context.Favorites.AnyAsync(f => f.UserId == userA.Id, ct));
            Assert.True(await context.Favorites.AnyAsync(f => f.UserId == userB.Id && f.ProductId == 1, ct));
        }

        #endregion RemoveFavoriteAsync

        #region Helpers

        private static FavoriteService CreateSut(ApplicationDbContext context, IProductService productService)
        {
            return new FavoriteService(context, productService, NullLogger<FavoriteService>.Instance);
        }

        private static ProductDto BuildProduct(int id)
        {
            return new ProductDto
            {
                Id = id,
                Title = $"Product {id}",
                Price = 9.99m * id,
                Stock = 10,
            };
        }

        #endregion Helpers
    }
}
