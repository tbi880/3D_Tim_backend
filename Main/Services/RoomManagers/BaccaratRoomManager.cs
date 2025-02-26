namespace _3D_Tim_backend.Services

{
    using _3D_Tim_backend.Enums;
    using _3D_Tim_backend.Repositories;
    using _3D_Tim_backend.Exceptions;
    using _3D_Tim_backend.Domain;


    public class BaccaratRoomManager : RoomManager
    {
        public BaccaratRoomManager(IUserRepository userRepository, RoomStorage storage) : base(userRepository, storage)
        {
        }

        public async Task<List<List<string>>> GetBaccaratHandsAsync(int roomId)
        {
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

        public override async Task<bool> PlaceBetsAsync(int roomId, int userId, Dictionary<string, int> BetSidesWithAmount)
        {
            if (!_storage.Rooms.TryGetValue(roomId, out var room))
            {
                throw new RoomNotFoundException(roomId);
            }
            if (!room.Users.TryGetValue(userId, out var userInRoom))
            {
                throw new RoomUserNotFoundException(userId, roomId);
            }

            if (BetSidesWithAmount.TryGetValue("Player", out int playerBetAmount))
            {
                await room.PlaceBetAsync(userId, "Player", playerBetAmount);
                BetSidesWithAmount.Remove("Player");
            }
            if (BetSidesWithAmount.TryGetValue("Banker", out int bankerBetAmount))
            {
                await room.PlaceBetAsync(userId, "Banker", bankerBetAmount);
                BetSidesWithAmount.Remove("Banker");
            }
            if (BetSidesWithAmount.TryGetValue("Tie", out int tieBetAmount))
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
