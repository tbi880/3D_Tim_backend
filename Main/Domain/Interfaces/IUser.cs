using System.Collections.Concurrent;

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
        int MoneyInRoom { get; set; }
        ConcurrentDictionary<string, int> BetSides { get; set; }
    }
}
