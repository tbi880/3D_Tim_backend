using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using _3D_Tim_backend.DTOs;
using _3D_Tim_backend.Services;
using System.Security.Claims;
using Microsoft.AspNetCore.Http.HttpResults;
using System.Threading.Tasks;

namespace _3D_Tim_backend.Controllers
{
    [ApiController]
    [Route("auth")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] UserRegisterDTO dto)
        {
            try
            {
                await _authService.RegisterAsync(dto);
                return Created();
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPost("login")]
        public IActionResult Login([FromBody] UserLoginDTO dto)
        {
            try
            {
                var token = _authService.LoginAsync(dto).Result;
                return Ok(new { Email = dto.Email, JwtToken = token });
            }
            catch (Exception ex)
            {
                return Unauthorized(new { message = ex.Message });
            }
        }

        [HttpPost("logout")]
        [Authorize]
        public IActionResult Logout()
        {
            var email = User.FindFirst(ClaimTypes.Name)?.Value;
            _authService.LogoutAsync(email);
            return Ok(new { message = "Logout successful" });
        }
    }
}
