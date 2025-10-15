using Microsoft.AspNetCore.SignalR;
using System.Collections.Concurrent;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication.JwtBearer;

[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
public class RoomHub : Hub
{
    private static readonly ConcurrentDictionary<int, string> UserToConnectionMap = new();
    private readonly ILogger<RoomHub> _logger;

    public RoomHub(ILogger<RoomHub> logger)
    {
        _logger = logger;
    }

    public override async Task OnConnectedAsync()
    {
        _logger.LogInformation("Connected: {ConnectionId}", Context.ConnectionId);
        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        _logger.LogInformation("Disconnected: {ConnectionId}", Context.ConnectionId);
        var userId = UserToConnectionMap.FirstOrDefault(x => x.Value == Context.ConnectionId).Key;
        if (userId != 0)
        {
            UserToConnectionMap.TryRemove(userId, out _);
        }
        await base.OnDisconnectedAsync(exception);
    }

    public async Task JoinRoom(int userId, int roomId)
    {
        UserToConnectionMap[userId] = Context.ConnectionId;

        await Groups.AddToGroupAsync(Context.ConnectionId, $"Room_{roomId}");
        _logger.LogInformation("User {UserId} joined room {RoomId}", userId, roomId);

        await Clients.Group($"Room_{roomId}").SendAsync("UserJoined", userId);
    }

    public async Task LeaveRoom(int userId, int roomId)
    {
        if (UserToConnectionMap.TryGetValue(userId, out var connectionId))
        {
            await Groups.RemoveFromGroupAsync(connectionId, $"Room_{roomId}");
            UserToConnectionMap.TryRemove(userId, out _);

            _logger.LogInformation("User {UserId} left room {RoomId}", userId, roomId);
            await Clients.Group($"Room_{roomId}").SendAsync("UserLeft", userId);
        }
    }

    public async Task SendMessageToRoom(int roomId, string message)
    {
        await Clients.Group($"Room_{roomId}").SendAsync("ReceiveMessage", message);
    }
}
