using Commons.Security.Service;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Commons.Security.Model;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.Extensions.Options;

namespace NotesServiceTests
{
    public class TokenServiceFixture
    {
        public ITokenService TokenService { get; }

        public TokenServiceFixture()
        {
            // Création d'une instance de JwtSettings
            var jwtSettings = new JwtSettings
            {
                // Configure les propriétés nécessaires ici
                Issuer = "DiabeteDetection",
                Audience = "DiabeteDetectionUsers",
                SecretKey = "MySuperSecretKey_12345MySuperSecretKey!"
            };

            var options = Options.Create(jwtSettings);

            // Passe l'instance d'options au constructeur de TokenService
            TokenService = new TokenService(options);
        }
    }
}
