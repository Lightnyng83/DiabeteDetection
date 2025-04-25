using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Commons.Security.Model;
using Commons.Security.Service;

namespace Commons.Security
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddJwtCommons(
            this IServiceCollection services,
            IConfiguration config)
        {
            var jwtSection = config.GetSection("JwtSettings");
            services.Configure<JwtSettings>(options => jwtSection.Bind(options));

            services.AddSingleton<ITokenService, TokenService>();

            return services;
        }
    }
}