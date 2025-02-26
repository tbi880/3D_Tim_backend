using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using _3D_Tim_backend.DTOs;
using _3D_Tim_backend.Services;
using System.Security.Claims;
using Microsoft.AspNetCore.Http.HttpResults;
using System.Threading.Tasks;
using _3D_Tim_backend.Exceptions;

namespace _3D_Tim_backend.Controllers
{
    [ApiController]
    [Route("room")]
    public class RoomController : ControllerBase
    {
        private readonly RoomManager _roomManager;

        public RoomController(RoomManager roomManager)
        {
            _roomManager = roomManager;
        }

        [HttpGet("all")]
        [Authorize]
        public async Task<IActionResult> GetAllRooms()
        {
            return Ok(await _roomManager.GetAllRoomsAsync());
        }

        [HttpPost("enter")]
        [Authorize]
        public async Task<IActionResult> EnterRoom([FromBody] EnterRoomDTO dto)
        {
            var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(userIdString, out int userId))
            {
                return BadRequest("Invalid user id");
            }
            try
            {
                if (await _roomManager.AddUserToRoomAsync(userId, dto.RoomId))
                {
                    return Ok();
                }
                else
                {
                    return BadRequest("Unexpected error");
                }
            }
            catch (RoomNotFoundException)
            {
                return NotFound("Room not found");
            }
            catch (RoomFullException)
            {
                return BadRequest("Room is full");
            }
            catch (RoomAlreadyHasTheUserException)
            {
                return BadRequest("User is already in room");
            }
        }

        [HttpPost("create")]
        [Authorize]
        public async Task<IActionResult> CreateRoom([FromBody] CreateRoomDTO roomDto)
        {
            var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            int roomId = 0;
            try
            {
                roomId = await _roomManager.CreateRoomAsync(roomDto.RoomName, roomDto.MaxUsers, roomDto.LevelOfBets, roomDto.GameType);
                if (int.TryParse(userIdString, out int userId) && await _roomManager.AddUserToRoomAsync(userId, roomId))
                {
                    return Ok(new CreateRoomReturnDTO(roomDto.RoomName, roomId, roomDto.GameType));
                }
                return BadRequest("Unexpected error");
            }
            catch (Exception ex)
            {
                await _roomManager.RemoveRoomAsync(roomId);
                return BadRequest(new { message = ex.Message });
            }

        }

        [HttpPost("exit")]
        [Authorize]
        public async Task<IActionResult> ExitRoom([FromBody] EnterRoomDTO roomDto)
        {
            var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            try
            {
                if (await _roomManager.RoomRemoveUserAsync(int.Parse(userIdString), roomDto.RoomId))
                {
                    return Ok();
                }
                else
                {
                    return BadRequest("Unexpected error");
                }
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }

        }



    }
}
