using AbySalto.Mid.Application.Baskets.Services;
using AbySalto.Mid.Application.Favorites.Services;
using AbySalto.Mid.Application.Products.Services;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace AbySalto.Mid.Application
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddApplication(this IServiceCollection services)
        {
            services.AddValidatorsFromAssembly(typeof(DependencyInjection).Assembly);
            services.AddScoped<IProductService, ProductService>();
            services.AddScoped<IFavoriteService, FavoriteService>();
            services.AddScoped<IBasketService, BasketService>();

            return services;
        }
    }
}
