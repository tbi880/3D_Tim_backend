namespace _3D_Tim_backend.Domain

{
    using System.Collections.Concurrent;
    using _3D_Tim_backend.Enums;

    public interface IRoom
    {
        int RoomId { get; set; }
        string RoomName { get; set; }
        int MaxUsers { get; set; }
        int RoomMinBet { get; set; }
        int RoomMaxBet { get; set; }
        int RoomUnitBet { get; set; }
        object Lock { get; }


        GameType GameType { get; set; }
        ConcurrentDictionary<int, IUser> Users { get; set; }

        Task StartGameAsync();
        Task<bool> IsFullAsync();
        Task PlaceBetAsync(int userId, string betSide, int betAmount);
        Task ClearBetAsync(int userId);
        void StartGameLoop();
        Task StopGameLoopAsync();
        Task RunGameLoopAsync(CancellationToken cancellationToken);
    }
}
