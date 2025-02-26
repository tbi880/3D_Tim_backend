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
    [Route("room/baccarat-room")]
    public class BaccaratRoomController : ControllerBase
    {
        private readonly BaccaratRoomManager _baccaratRoomManager;

        public BaccaratRoomController(BaccaratRoomManager baccaratRoomManager)
        {
            _baccaratRoomManager = baccaratRoomManager;
        }

        [HttpGet("cards")]
        [Authorize]
        public async Task<IActionResult> GetCards([FromBody] EnterRoomDTO roomDto)
        {
            var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(userIdString, out int userId))
            {
                return BadRequest("Invalid user id");
            }
            var room = await _baccaratRoomManager.GetRoomAsync(roomDto.RoomId);
            if (room == null || !room.Users.ContainsKey(userId))
            {
                return NotFound("User not found in room");
            }
            try
            {
                return Ok(await _baccaratRoomManager.GetBaccaratHandsAsync(roomDto.RoomId));
            }
            catch (RoomUserNotFoundException)
            {
                return NotFound("User not found in room");
            }
            catch (NoCardInShoeException)
            {
                return BadRequest("No card in shoe");
            }
            catch (BaccaratHandsNotAvailableYetException)
            {
                return BadRequest("Hands not available yet, please retry in a second!");
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("result-board")]
        [Authorize]
        public async Task<IActionResult> GetResultBoard([FromBody] EnterRoomDTO roomDto)
        {
            var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(userIdString, out int userId))
            {
                return BadRequest("Invalid user id");
            }
            var room = await _baccaratRoomManager.GetRoomAsync(roomDto.RoomId);
            if (room == null || !room.Users.ContainsKey(userId))
            {
                return NotFound("User not found in room");
            }
            return Ok(await _baccaratRoomManager.GetBaccaratResultListAsync(roomDto.RoomId));
        }

        [HttpGet("winning-sides")]
        [Authorize]
        public async Task<IActionResult> GetWinningSides([FromBody] EnterRoomDTO roomDto)
        {
            var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(userIdString, out int userId))
            {
                return BadRequest("Invalid user id");
            }
            var room = await _baccaratRoomManager.GetRoomAsync(roomDto.RoomId);
            if (room == null || !room.Users.ContainsKey(userId))
            {
                return NotFound("User not found in room");
            }
            try
            {
                return Ok(await _baccaratRoomManager.GetBaccaratWinningSidesAsync(roomDto.RoomId));
            }
            catch (RoomUserNotFoundException)
            {
                return NotFound("User not found in room");
            }
            catch (NoCardInShoeException)
            {
                return BadRequest("No card in shoe");
            }
            catch (BaccaratHandsNotAvailableYetException)
            {
                return BadRequest("Hands not available yet, please retry in a second!");
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
        [HttpPost("place-bets")]
        [Authorize]
        public async Task<IActionResult> PlaceBets([FromBody] PlaceBetsDTO betsDTO)
        {
            var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(userIdString, out int userId))
            {
                return BadRequest("Invalid user id");
            }
            var room = await _baccaratRoomManager.GetRoomAsync(betsDTO.RoomId);
            if (room == null || !room.Users.ContainsKey(userId))
            {
                return NotFound("User not found in room");
            }
            if (betsDTO.BetSidesWithAmount.Count == 0)
            {
                return BadRequest("No bet placed");
            }
            try
            {
                if (await _baccaratRoomManager.PlaceBetsAsync(betsDTO.RoomId, userId, betsDTO.BetSidesWithAmount))
                { return Ok(); }
                return BadRequest("Failed to place bets");
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }

        }
    }
}