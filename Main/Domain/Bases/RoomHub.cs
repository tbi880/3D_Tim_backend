using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

public class RoomHub : Hub
{
    private static readonly ConcurrentDictionary<string, int> ConnectionToUserMap = new();
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

    public override async Task OnDisconnectedAsync(Exception exception)
    {
        if (ConnectionToUserMap.TryRemove(Context.ConnectionId, out int userId))
        {
            UserToConnectionMap.TryRemove(userId, out _);
            _logger.LogInformation("User {UserId} disconnected", userId);
        }
        await base.OnDisconnectedAsync(exception);
    }

    public async Task JoinRoom(int userId, int roomId)
    {
        ConnectionToUserMap[Context.ConnectionId] = userId;
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
            ConnectionToUserMap.TryRemove(connectionId, out _);
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
