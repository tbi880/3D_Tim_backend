using Microsoft.AspNetCore.SignalR;
using System.Collections.Concurrent;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using System.Security.Claims;
using _3D_Tim_backend.Services;
using _3D_Tim_backend.Enums;

[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
public class RoomHub : Hub
{
    private readonly ILogger<RoomHub> _logger;
    private readonly RoomManager _roomManager;

    public RoomHub(ILogger<RoomHub> logger, RoomManager roomManager)
    {
        _logger = logger;
        _roomManager = roomManager;
    }

    public override async Task OnConnectedAsync()
    {
        _logger.LogInformation("Connected: {ConnectionId}", Context.ConnectionId);

        var userIdString = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!int.TryParse(userIdString, out int userId))
        {
            _logger.LogWarning("Invalid user id for connection {ConnectionId}", Context.ConnectionId);
            return;
        }
        var roomId = await _roomManager.GetRoomIdByUserIdAsync(userId);
        if (roomId == null)
        {
            _logger.LogWarning("User {UserId} not found in any room on disconnect", userId);
            return;
        }
        await Clients.Group($"Room_{roomId}").SendAsync("UserJoined", userId);

        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        _logger.LogInformation("Disconnected: {ConnectionId}", Context.ConnectionId);
        var userIdString = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!int.TryParse(userIdString, out int userId))
        {
            _logger.LogWarning("Invalid user id for connection {ConnectionId}", Context.ConnectionId);
            return;
        }
        var roomId = await _roomManager.GetRoomIdByUserIdAsync(userId);
        if (roomId == null)
        {
            _logger.LogWarning("User {UserId} not found in any room on disconnect", userId);
            await base.OnDisconnectedAsync(exception);
            return;
        }
        await _roomManager.RoomRemoveUserAsync(userId, (int)roomId);
        await base.OnDisconnectedAsync(exception);
    }

    public async Task SendMessageToEveryoneInRoom(int roomId, string message)
    {
        await Clients.Group($"Room_{roomId}").SendAsync("ReceiveMessage", message);
    }

    public async Task UserActionChangeUserStatusInRoom(int roomId, string status)
    {
        var userIdString = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!int.TryParse(userIdString, out int userId))
        {
            _logger.LogWarning("Invalid user id for connection {ConnectionId}", Context.ConnectionId);
            return;
        }

        try
        {
            var user = await _roomManager.GetUserInRoomAsync(roomId, userId);
            if (user == null)
            {
                _logger.LogWarning("User {UserId} not found in room {RoomId}", userId, roomId);
                throw new HubException("User not found in room");
            }

            user.StatusInRoom = status switch
            {
                "waiting" => RoomUserStatus.waiting,
                "betting" => RoomUserStatus.betting,
                "dealing" => RoomUserStatus.dealing,
                "results" => RoomUserStatus.results,
                _ => user.StatusInRoom
            };

            _logger.LogInformation("User {UserId} changed status to {Status} in room {RoomId}", userId, status, roomId);

            await Clients.Group($"Room_{roomId}").SendAsync("UserStatusChanged", new
            {
                userId,
                status
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to change user status in room {RoomId} for user {UserId}", roomId, userId);
            throw new HubException("Failed to change user status");
        }
    }
}
