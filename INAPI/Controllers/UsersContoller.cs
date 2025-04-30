using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Services;
using Storage;
using System.Threading.Tasks;
using Microsoft.IdentityModel.Tokens;
using Models.Entities;

namespace ItmoNoteAPI.Controllers
{
    [Route("api/users")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly IConfiguration _configuration;

        public UsersController(IUserService userService, IConfiguration configuration)
        {
            _userService = userService;
            _configuration = configuration;
        }

        [HttpPost("auth/telegram")]
        public async Task<ActionResult> AuthenticateTelegram([FromBody] TelegramAuthRequest request)
        {
            try
            {
                // Аутентификация через UserService с проверкой хэша
                var user = await _userService.AuthenticationAsync(
                    request.Id,
                    request.FirstName,
                    request.LastName,
                    request.Username,
                    request.Hash,
                    request.AuthDate);

                if (user == null)
                {
                    return BadRequest("Ошибка аутентификации");
                }

                // Генерация JWT-токена
                var token = GenerateJwtToken(user);
                
                return Ok(new { Token = token });
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message); // Возвращаем сообщение об ошибке
            }
        }
        private string GenerateJwtToken(User user)
        {
            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.Username ?? user.FirstName),
                new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var expires = DateTime.Now.AddMinutes(Convert.ToDouble(_configuration["Jwt:ExpiryInMinutes"]));

            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claims,
                expires: expires,
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }

    public class TelegramAuthRequest
    {
        public int Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Username { get; set; }
        public long AuthDate { get; set; }
        public string Hash { get; set; }
    }
}