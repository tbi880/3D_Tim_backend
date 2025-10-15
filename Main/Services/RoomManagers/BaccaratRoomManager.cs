namespace _3D_Tim_backend.Services

{
    using _3D_Tim_backend.Enums;
    using _3D_Tim_backend.Repositories;
    using _3D_Tim_backend.Exceptions;
    using _3D_Tim_backend.Domain;
    using Microsoft.Extensions.Logging;


    public class BaccaratRoomManager : RoomManager
    {
        private readonly ILogger<BaccaratRoomManager> _logger;

        public BaccaratRoomManager(IUserRepository userRepository, RoomStorage storage, ILogger<RoomManager> baseLogger, ILogger<BaccaratRoomManager> logger) : base(userRepository, storage, baseLogger)
        {
            _logger = logger;
        }

        public async Task<List<List<string>>> GetBaccaratHandsAsync(int roomId)
        {
            _logger.LogInformation("Retrieving baccarat hands for room {RoomId}", roomId);
            if (!_storage.Rooms.TryGetValue(roomId, out var room))
            {
                throw new RoomNotFoundException(roomId);
            }
            if (room.GameType != GameType.Baccarat)
            {
                throw new GameTypeNotValidException(room.GameType.ToString());
            }
            try
            {
                BaccaratRoom baccaratRoom = (BaccaratRoom)room;
                List<List<string>> hands = await baccaratRoom.GetLatestGameHandsAsync();
                return hands;
            }
            catch (BaccaratHandsNotAvailableYetException)
            {
                throw new BaccaratHandsNotAvailableYetException();
            }
        }

        public async Task<List<string>> GetBaccaratWinningSidesAsync(int roomId)
        {
            _logger.LogInformation("Retrieving baccarat winning sides for room {RoomId}", roomId);
            if (!_storage.Rooms.TryGetValue(roomId, out var room))
            {
                throw new RoomNotFoundException(roomId);
            }
            if (room.GameType != GameType.Baccarat)
            {
                throw new GameTypeNotValidException(room.GameType.ToString());
            }
            try
            {
                BaccaratRoom baccaratRoom = (BaccaratRoom)room;
                List<string> winningSides = await baccaratRoom.GetLatestGameWinningSidesAsync();
                return winningSides;
            }
            catch (BaccaratWinningSidesNotAvailableYetException)
            {
                throw new BaccaratWinningSidesNotAvailableYetException();
            }
        }

        public async Task<List<string>> GetBaccaratResultListAsync(int roomId)
        {
            _logger.LogInformation("Retrieving baccarat result list for room {RoomId}", roomId);
            if (!_storage.Rooms.TryGetValue(roomId, out var room))
            {
                throw new RoomNotFoundException(roomId);
            }
            if (room.GameType != GameType.Baccarat)
            {
                throw new GameTypeNotValidException(room.GameType.ToString());
            }
            BaccaratRoom baccaratRoom = (BaccaratRoom)room;
            List<string> resultList = await baccaratRoom.GetResultListAsync();
            return resultList;
        }

        public override async Task<bool> PlaceBetsAsync(int roomId, int userId, Dictionary<string, long> BetSidesWithAmount)
        {
            _logger.LogInformation("Placing baccarat bets for user {UserId} in room {RoomId}", userId, roomId);
            if (!_storage.Rooms.TryGetValue(roomId, out var room))
            {
                throw new RoomNotFoundException(roomId);
            }
            if (!room.Users.TryGetValue(userId, out var userInRoom))
            {
                throw new RoomUserNotFoundException(userId, roomId);
            }

            if (BetSidesWithAmount.TryGetValue("Player", out long playerBetAmount))
            {
                await room.PlaceBetAsync(userId, "Player", playerBetAmount);
                BetSidesWithAmount.Remove("Player");
            }
            if (BetSidesWithAmount.TryGetValue("Banker", out long bankerBetAmount))
            {
                await room.PlaceBetAsync(userId, "Banker", bankerBetAmount);
                BetSidesWithAmount.Remove("Banker");
            }
            if (BetSidesWithAmount.TryGetValue("Tie", out long tieBetAmount))
            {
                await room.PlaceBetAsync(userId, "Tie", tieBetAmount);
                BetSidesWithAmount.Remove("Tie");
            }
            foreach (var (betSide, betAmount) in BetSidesWithAmount)
            {
                await room.PlaceBetAsync(userId, betSide, betAmount);
            }
            return true;
        }

    }
}
