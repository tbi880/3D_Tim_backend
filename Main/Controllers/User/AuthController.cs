using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using _3D_Tim_backend.DTOs;
using System.Security.Claims;
using _3D_Tim_backend.Services;

namespace _3D_Tim_backend.Controllers
{
    [ApiController]
    [Route("auth")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly RoomManager _roomManager;

        public AuthController(IAuthService authService, RoomManager roomManager)
        {
            _authService = authService;
            _roomManager = roomManager;
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

        [HttpPost("guest-login")]
        public async Task<IActionResult> GuestLogin([FromBody] TempUserRegisterDTO tempUserRegisterDTO)
        {
            try
            {
                var token = await _authService.RegisterGuestUserAsync(tempUserRegisterDTO);
                return Ok(new TempUserRegisterReturnDTO(tempUserRegisterDTO.Name, tempUserRegisterDTO.Email, token));

            }
            catch (Exception ex)
            {
                if (ex.Message == "User already exists as a registered user")
                {
                    return Unauthorized(new { message = ex.Message });
                }
                return Unauthorized(new { message = ex.Message });
            }

        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] UserLoginDTO dto)
        {
            try
            {
                var token = await _authService.LoginAsync(dto);
                return Ok(new { Email = dto.Email, JwtToken = token });
            }
            catch (Exception ex)
            {
                return Unauthorized(new { message = ex.Message });
            }
        }

        [HttpGet("profile")]
        [Authorize]
        public async Task<IActionResult> GetProfile()
        {
            var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(userIdString, out int userId))
            {
                return BadRequest("Invalid user id");
            }
            var roomId = await _roomManager.GetRoomIdByUserIdAsync(userId);
            if (roomId != null)
            {
                await _roomManager.RoomRemoveUserAsync(userId, (int)roomId);
            }
            var userEntity = await _authService.GetUserByIdAsync(userId);
            UserDTO userDTO = new UserDTO(
    userEntity.Id,
    userEntity.Name,
    userEntity.Email,
    userEntity.Money,
    userEntity.TotalBets,
    userEntity.Role.ToString()
);
            return Ok(userDTO);
        }

        [HttpPost("logout")]
        [Authorize]
        public async Task<IActionResult> Logout()
        {
            var email = User.FindFirst(ClaimTypes.Email)?.Value;
            await _authService.LogoutAsync(email);
            return Ok(new { message = "Logout successful" });
        }

#if DEBUG // only in development environment

        [HttpDelete("delete")]
        public async Task<IActionResult> DeleteAllAccountAsync()
        {
            await _authService.DeleteAllAccountAsync();
            return Ok();
        }

#endif
    }
}
