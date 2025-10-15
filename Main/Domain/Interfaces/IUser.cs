using System.Collections.Concurrent;
using _3D_Tim_backend.Enums;

namespace _3D_Tim_backend.Domain

{
    public interface IUser
    {
        int UserId { get; set; }
        string UserName { get; set; }
        int WinCountInRoom { get; set; }
        int LoseCountInRoom { get; set; }
        int TieCountInRoom { get; set; }
        int TotalBetsInRoom { get; set; }
        long MoneyInRoom { get; set; }
        RoomUserStatus StatusInRoom { get; set; }
        ConcurrentDictionary<string, long> BetSides { get; set; }
    }
}
