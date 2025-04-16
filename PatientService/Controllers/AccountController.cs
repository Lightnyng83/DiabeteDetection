using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System;
using System.Threading.Tasks;
using PatientService.Models;
using PatientService.Security; // Pour ApplicationUser

namespace PatientService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class HealthController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        public HealthController(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }
        // GET: api/health
        [HttpGet]
        public IActionResult GetHealth() => Ok("Healthy");
    }


    [Route("api/[controller]")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly JwtSettings _jwtSettings;

        public AccountController(UserManager<ApplicationUser> userManager, IOptions<JwtSettings> jwtSettings)
        {
            _userManager = userManager;
            _jwtSettings = jwtSettings.Value;
        }

        // POST: api/account/login
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Vérifier que l'utilisateur existe et que le mot de passe est correct
            var user = await _userManager.FindByNameAsync(model.Username);
            if (user == null || !(await _userManager.CheckPasswordAsync(user, model.Password)))
            {
                return Unauthorized("Identifiants invalides");
            }

            // Générer le token JWT
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_jwtSettings.SecretKey);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new Claim[]
                {
                    new Claim(ClaimTypes.NameIdentifier, user.Id),
                    new Claim(ClaimTypes.Name, user.UserName!)
                }),
                Expires = DateTime.UtcNow.AddMinutes(_jwtSettings.ExpiryInMinutes),
                Issuer = _jwtSettings.Issuer,
                Audience = _jwtSettings.Audience,
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key),
                    SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            var tokenString = tokenHandler.WriteToken(token);

            // Configurer les options du cookie HTTP-only
            var cookieOptions = new CookieOptions
            {
                HttpOnly = true,
                Secure = true, // Nécessite HTTPS
                SameSite = SameSiteMode.None, // IMPORTANT pour cross-origin
                Expires = DateTime.UtcNow.AddMinutes(_jwtSettings.ExpiryInMinutes)
            };
            HttpContext.Response.Cookies.Append("token", tokenString, cookieOptions);


            return Ok(new { Message = "Authentification réussie", Token = tokenString });
        }
    }

    public class LoginRequest
    {
        public required string Username { get; set; }
        public required string Password { get; set; }
    }
}
