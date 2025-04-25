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
            // On récupère la section "JwtSettings" et on la lie au POCO  
            var jwtSection = config.GetSection("JwtSettings");
            services.Configure<JwtSettings>(options => jwtSection.Bind(options));

            // Enregistrement de votre ITokenService/TokenService communs  
            services.AddSingleton<ITokenService, TokenService>();

            return services;
        }
    }
}