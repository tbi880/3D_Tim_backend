using System.Collections.Concurrent;

namespace _3D_Tim_backend.Domain
{
    public class User : IUser
    {
        public int UserId { get; set; }
        public string UserName { get; set; } = string.Empty;
        public int WinCountInRoom { get; set; } = 0;
        public int LoseCountInRoom { get; set; } = 0;
        public int TieCountInRoom { get; set; } = 0;
        public int TotalBetsInRoom { get; set; } = 0;
        public long MoneyInRoom { get; set; } = 0;

        public ConcurrentDictionary<string, long> BetSides { get; set; } = new ConcurrentDictionary<string, long>();


    }
}
