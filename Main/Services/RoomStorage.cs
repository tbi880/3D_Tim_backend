using System.Collections.Concurrent;
using _3D_Tim_backend.Domain;

public class RoomStorage
{
    public ConcurrentDictionary<int, IRoom> Rooms { get; } = new ConcurrentDictionary<int, IRoom>();
    public ConcurrentDictionary<int, int> UserIdToRoomId { get; } = new ConcurrentDictionary<int, int>();
}
