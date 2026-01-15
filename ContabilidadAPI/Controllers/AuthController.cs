using CapaNegocio.ContabilidadAPI.Models;
using CapaNegocio.ContabilidadAPI.Repository.Interfaces.Access;
using ContabilidadAPI.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace ContabilidadAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IConfiguration Configuration;
        private readonly IAccessService _accessService;

        public AuthController(IConfiguration configuration, IAccessService accessService)
        {
            Configuration = configuration;
            _accessService = accessService;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] UserLogin user)
        {
            try
            {
                var userData = await _accessService.ValidarPersonal(user.Username, user.Password);
                if (userData.Success)
                {
                    var token = GenerateJwtToken(user.Username);
                    userData.Data.token = token;
                    return Ok(userData);
                }
                return Unauthorized(userData);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponse<string>("Error interno del servidor: " + ex.Message));
            }

        }

        [Authorize]
        [HttpPost("GetPerfilByDni")]
        public async Task<IActionResult> GetPerfilByDni(string idDocumento)
        {
            try
            {
                var item = await _accessService.GetPerfilesByUsuario(idDocumento);
                if (item.Data == null)
                    return NotFound(item);
                
                return Ok(item);

            }
            catch (Exception ex) {
                return StatusCode(500, new ApiResponse<string>("Error interno del servidor: " + ex.Message));
            }            
        }

        private string GenerateJwtToken(string username)
        {
            var claims = new[] {
                new Claim(JwtRegisteredClaimNames.Sub, username),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Configuration["JwtSecurityToken:key"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var issuer = Configuration["JwtSecurityToken:issuer"]; 
            var audiences = Configuration["JwtSecurityToken:audience"]?.Split(',') ?? new string[0];

            var token = new JwtSecurityToken(
             issuer: issuer, 
             audience: audiences.FirstOrDefault(),
             claims: claims,
             expires: DateTime.Now.AddDays(30),
             signingCredentials: creds);
             
            token.Payload[JwtRegisteredClaimNames.Aud] = audiences.Length > 1 ? audiences : audiences.FirstOrDefault();

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

    }
}
