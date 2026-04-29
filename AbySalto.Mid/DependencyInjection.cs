using Microsoft.OpenApi;

namespace AbySalto.Mid
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddPresentation(this IServiceCollection services)
        {
            services.AddControllers();
            services.AddOpenApi();
            services.AddEndpointsApiExplorer();
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "AbySalto Mid", Version = "v1" });
            });

            return services;
        }
    }
}
