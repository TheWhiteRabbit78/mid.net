using AbySalto.Mid.Application.Authentication.Services;
using AbySalto.Mid.Application.Common.Persistence;
using AbySalto.Mid.Application.Products.Models;
using AbySalto.Mid.Application.Products.Services;
using AbySalto.Mid.Domain.Identity;
using AbySalto.Mid.Infrastructure.Authentication;
using AbySalto.Mid.Infrastructure.External.DummyJson;
using AbySalto.Mid.Infrastructure.Persistence;
using Microsoft.AspNetCore.Identity;

namespace AbySalto.Mid.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddDatabase(configuration);
            services.AddIdentityServices();
            services.AddAuthenticationServices(configuration);
            services.AddExternalClients(configuration);
            services.AddCaching();

            return services;
        }

        private static IServiceCollection AddDatabase(this IServiceCollection services, IConfiguration configuration)
        {
            string connectionString = configuration.GetConnectionString("DefaultConnection")
                ?? throw new InvalidOperationException("Connection string 'DefaultConnection' is not configured.");

            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseNpgsql(connectionString));

            services.AddScoped<IApplicationDbContext>(provider => provider.GetRequiredService<ApplicationDbContext>());

            return services;
        }

        private static IServiceCollection AddIdentityServices(this IServiceCollection services)
        {
            // AddIdentityCore over AddDefaultIdentity — this is an API, no cookie-based UI flows needed
            services.AddIdentityCore<ApplicationUser>(options =>
            {
                options.User.RequireUniqueEmail = true;

                options.Password.RequireDigit = true;
                options.Password.RequireLowercase = true;
                options.Password.RequireUppercase = true;
                options.Password.RequireNonAlphanumeric = true;
                options.Password.RequiredLength = 8;
            })
            .AddRoles<ApplicationRole>()
            .AddEntityFrameworkStores<ApplicationDbContext>();

            return services;
        }

        private static IServiceCollection AddAuthenticationServices(this IServiceCollection services, IConfiguration configuration)
        {
            services.Configure<JwtSettings>(configuration.GetSection(JwtSettings.SectionName));
            services.AddSingleton<IJwtTokenService, JwtTokenService>();

            return services;
        }

        private static IServiceCollection AddExternalClients(this IServiceCollection services, IConfiguration configuration)
        {
            services.Configure<DummyJsonSettings>(configuration.GetSection(DummyJsonSettings.SectionName));

            DummyJsonSettings settings = configuration
                .GetSection(DummyJsonSettings.SectionName)
                .Get<DummyJsonSettings>()
                ?? new DummyJsonSettings();

            services.AddHttpClient<IDummyJsonClient, DummyJsonClient>(client =>
            {
                client.BaseAddress = new Uri(settings.BaseUrl);
            });

            return services;
        }

        private static IServiceCollection AddCaching(this IServiceCollection services)
        {
            services.AddMemoryCache();
            return services;
        }
    }
}
