using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Net.Http;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Extensions.Configuration;
using System.IdentityModel.Tokens.Jwt;
using Commons.Security.Model;
using Microsoft.Extensions.Options;

namespace Commons.Security.Service
{
    public class TokenService : ITokenService
    {
        private readonly JwtSettings _jwt;

        public TokenService(IOptions<JwtSettings> options)
        {
            _jwt = options.Value;
        }
        private string GenerateTestToken(int expiryMinutes = 60, IEnumerable<Claim>? customClaims = null)
        {
            var key = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(_jwt.SecretKey));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new List<Claim>
            {
                new(ClaimTypes.NameIdentifier, "test-user-id"),
                new(ClaimTypes.Name, "testuser")
            };

            if (customClaims != null)
            {
                claims.AddRange(customClaims);
            }

            var token = new JwtSecurityToken(
                issuer: _jwt.Issuer,
                audience: _jwt.Audience,
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(expiryMinutes),
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        public void AuthenticateClient(HttpClient client, int expiryMinutes = 60)
        {
            var token = GenerateTestToken(expiryMinutes);
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }
    }
}



