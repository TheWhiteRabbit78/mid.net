using AbySalto.Mid.Application.Authentication.Services;
using AbySalto.Mid.Domain.Identity;
using AbySalto.Mid.Infrastructure.Authentication;
using AbySalto.Mid.Infrastructure.Persistence;

namespace AbySalto.Mid.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddDatabase(configuration);
            services.AddIdentityServices();
            services.AddAuthenticationServices(configuration);
            services.AddServices();

            return services;
        }

        private static IServiceCollection AddDatabase(this IServiceCollection services, IConfiguration configuration)
        {
            string connectionString = configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' is not configured.");

            services.AddDbContext<ApplicationDbContext>(options => options.UseNpgsql(connectionString));

            return services;
        }

        private static IServiceCollection AddIdentityServices(this IServiceCollection services)
        {
            // AddIdentityCore
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

        private static IServiceCollection AddServices(this IServiceCollection services)
        {
            return services;
        }
    }
}