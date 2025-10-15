namespace _3D_Tim_backend.Domain

{
    using System.Collections.Concurrent;
    using System.Threading;
    using _3D_Tim_backend.Enums;
    using _3D_Tim_backend.Exceptions;
    using _3D_Tim_backend.Services;

    public class Room : IRoom
    {
        public int RoomId { get; set; }
        public required string RoomName { get; set; }
        public int MaxUsers { get; set; }
        public ConcurrentDictionary<int, IUser> Users { get; set; } = new ConcurrentDictionary<int, IUser>();
        public long RoomMinBet { get; set; }
        public long RoomMaxBet { get; set; }
        public long RoomUnitBet { get; set; }
        public GameType GameType { get; set; }

        private readonly object _lock = new object();
        protected readonly IBetHandler _betHandler;
        public object Lock => _lock;


        public Room(int roomId, string roomName, IBetHandler betHandler, int maxUsers, long roomMinBet, long roomMaxBet, long roomUnitBet)
        {
            RoomId = roomId;
            RoomName = roomName;
            _betHandler = betHandler;
            MaxUsers = maxUsers;
            RoomMinBet = roomMinBet;
            RoomMaxBet = roomMaxBet;
            RoomUnitBet = roomUnitBet;
        }

        public async Task<bool> IsFullAsync() => Users.Count >= MaxUsers;


        public virtual async Task PlaceBetAsync(int userId, string betSide, long betAmount)
        {
            throw new NotImplementedException("PlaceBet must be implemented in the derived class.");
        }

        public virtual async Task ClearBetAsync(int userId)
        {
            throw new NotImplementedException("ClearBet must be implemented in the derived class.");
        }


        public virtual async Task StartGameAsync()
        {
            throw new NotImplementedException("StartGame must be implemented in the derived class.");
        }

        public virtual void StartGameLoop()
        {
            throw new NotImplementedException();
        }

        public virtual Task StopGameLoopAsync()
        {
            throw new NotImplementedException();
        }

        public virtual Task RunGameLoopAsync(CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

    }
}
