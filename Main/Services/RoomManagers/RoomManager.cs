namespace _3D_Tim_backend.Services

{
    using _3D_Tim_backend.Enums;
    using _3D_Tim_backend.Repositories;
    using _3D_Tim_backend.Exceptions;
    using _3D_Tim_backend.Domain;
    using System.Security.Cryptography;

    public class RoomManager
    {
        protected readonly IUserRepository _userRepository;
        protected readonly RoomStorage _storage;

        public RoomManager(IUserRepository userRepository, RoomStorage storage)
        {
            _userRepository = userRepository;
            _storage = storage;
        }

        public async Task RemoveRoomAsync(int roomId)
        {
            _storage.Rooms.TryGetValue(roomId, out var room);
            if (room == null)
            {
                throw new RoomNotFoundException(roomId);
            }
            await room.StopGameLoopAsync();
            _storage.Rooms.TryRemove(roomId, out _);
        }

        public async Task<IRoom?> GetRoomAsync(int roomId)
        {
            _storage.Rooms.TryGetValue(roomId, out var room);
            return room;
        }

        public async Task<IEnumerable<IRoom>> GetAllRoomsAsync()
        {
            return _storage.Rooms.Values;
        }

        public async Task<bool> AddUserToRoomAsync(int userId, int roomId)
        {
            if (!_storage.Rooms.ContainsKey(roomId))
            {
                throw new RoomNotFoundException(roomId);
            }

            IRoom room = _storage.Rooms[roomId];

            if (!_storage.UserIdToRoomId.ContainsKey(userId))
            {
                _storage.UserIdToRoomId.TryAdd(userId, roomId);
            }
            else
            {
                await RoomRemoveUserAsync(userId, _storage.UserIdToRoomId[userId]);
                _storage.UserIdToRoomId.TryAdd(userId, roomId);
            }
            return await RoomAddUserAsync(userId, roomId);
        }

        public async Task<int> CreateRoomAsync(string roomName, int maxUsers, string levelOfBets, GameType gameType)
        {
            if (!Enum.TryParse(levelOfBets, true, out RoomMinBet roomMinBet) || !Enum.TryParse(levelOfBets, true, out RoomMaxBet roomMaxBet) || !Enum.TryParse(levelOfBets, true, out RoomUnitBet roomUnitBet))
            {
                throw new LevelOfBetsNotValidException(levelOfBets);
            }

            int roomId;
            while (true)
            {
                roomId = RandomNumberGenerator.GetInt32(0, 9999);
                if (!_storage.Rooms.ContainsKey(roomId))
                {
                    break;
                }
            }
            Room room = gameType switch
            {
                GameType.Baccarat => new BaccaratRoom(roomId, roomName, new BaccaratBetHandler(), maxUsers, (int)roomMinBet, (int)roomMaxBet, (int)roomUnitBet) { RoomName = roomName },
            };
            _storage.Rooms.TryAdd(roomId, room);
            return roomId;
        }

        public async Task<bool> RoomAddUserAsync(int userId, int roomId)
        {
            if (!_storage.Rooms.ContainsKey(roomId))
            {
                throw new RoomNotFoundException(roomId);
            }
            IRoom room = _storage.Rooms[roomId];
            var userEntity = await _userRepository.GetByIdAsync(userId);
            if (userEntity == null)
            {
                throw new UserNotFoundException(userId);
            }
            lock (room.Lock)
            {
                if (room.Users.Count >= room.MaxUsers) throw new RoomFullException();

                if (room.Users.ContainsKey(userId)) throw new RoomAlreadyHasTheUserException(userId);

                if (userEntity.Money < room.RoomMinBet) throw new RoomEnterInsufficientBalanceException(userId, userEntity.Money, room.RoomMinBet);

                var userDomain = new User
                {
                    UserId = userEntity.Id,
                    UserName = userEntity.Name,
                    MoneyInRoom = userEntity.Money
                };
                room.Users.TryAdd(userId, userDomain);
            }
            userEntity.Money = 0;
            await _userRepository.UpdateUserAsync(userEntity);
            return true;
        }

        public async Task<bool> RoomRemoveUserAsync(int userId, int roomId)
        {
            if (!_storage.Rooms.TryGetValue(roomId, out var room))
            {
                throw new RoomNotFoundException(roomId);
            }
            if (!room.Users.TryGetValue(userId, out var userInRoom))
            {
                throw new RoomUserNotFoundException(userId, roomId);
            }
            await _userRepository.SyncUserDataToDbAsync(userInRoom);
            room.Users.TryRemove(userId, out _);
            _storage.UserIdToRoomId.TryRemove(userId, out _);

            if (room.Users.IsEmpty)
            {
                await RemoveRoomAsync(roomId);
            }
            return true;
        }

        public virtual async Task<bool> PlaceBetsAsync(int roomId, int userId, Dictionary<string, int> BetSidesWithAmount)
        {
            if (!_storage.Rooms.TryGetValue(roomId, out var room))
            {
                throw new RoomNotFoundException(roomId);
            }
            if (!room.Users.TryGetValue(userId, out var userInRoom))
            {
                throw new RoomUserNotFoundException(userId, roomId);
            }
            foreach (var (betSide, betAmount) in BetSidesWithAmount)
            {
                await room.PlaceBetAsync(userId, betSide, betAmount);
            }
            return true;
        }


    }
}
