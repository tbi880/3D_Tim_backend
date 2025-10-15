namespace _3D_Tim_backend.Services

{
    using _3D_Tim_backend.Enums;
    using _3D_Tim_backend.Repositories;
    using _3D_Tim_backend.Exceptions;
    using _3D_Tim_backend.Domain;
    using System.Security.Cryptography;
    using Microsoft.Extensions.Logging;
    using Microsoft.AspNetCore.SignalR;

    public class RoomManager
    {
        protected readonly IUserRepository _userRepository;
        protected readonly RoomStorage _storage;
        protected readonly IHubContext<RoomHub> _hubContext;
        protected readonly ILogger<RoomManager> _logger;

        public RoomManager(IUserRepository userRepository, RoomStorage storage, IHubContext<RoomHub> hubContext, ILogger<RoomManager> logger)
        {
            _userRepository = userRepository;
            _storage = storage;
            _hubContext = hubContext;
            _logger = logger;
        }

        public async Task RemoveRoomAsync(int roomId)
        {
            _logger.LogInformation("Removing room {RoomId}", roomId);
            if (!_storage.Rooms.TryGetValue(roomId, out var room) || room == null)
            {
                _logger.LogWarning("Tried to remove non-existent room {RoomId}", roomId);
                return;
            }

            await room.StopGameLoopAsync();
            _storage.Rooms.TryRemove(roomId, out _);
        }

        public async Task<IRoom?> GetRoomAsync(int roomId)
        {
            _logger.LogInformation("Getting room {RoomId}", roomId);
            _storage.Rooms.TryGetValue(roomId, out var room);
            return room;
        }

        public async Task<IEnumerable<IRoom>> GetAllRoomsAsync()
        {
            _logger.LogInformation("Getting all rooms");
            return _storage.Rooms.Values;
        }

        public async Task<bool> AddUserToRoomAsync(int userId, int roomId)
        {
            _logger.LogInformation("Adding user {UserId} to room {RoomId}", userId, roomId);
            if (!_storage.Rooms.ContainsKey(roomId))
            {
                throw new RoomNotFoundException(roomId);
            }

            if (_storage.UserIdToRoomId.TryGetValue(userId, out var existingRoomId))
            {
                try
                {
                    await RoomRemoveUserAsync(userId, existingRoomId);
                }
                catch (RoomNotFoundException)
                {
                    _storage.UserIdToRoomId.TryRemove(userId, out _);
                    _logger.LogWarning("Stale mapping removed for user {UserId} -> room {RoomId}", userId, existingRoomId);
                }
            }

            var added = await RoomAddUserAsync(userId, roomId);
            if (added)
            {
                _storage.UserIdToRoomId.TryAdd(userId, roomId);
            }
            return added;
        }

        public async Task<int> CreateRoomAsync(string roomName, int maxUsers, string levelOfBets, GameType gameType)
        {
            _logger.LogInformation("Creating room {RoomName}", roomName);
            if (!Enum.TryParse<RoomMinBet>(levelOfBets, true, out var roomMinBet) || !Enum.TryParse<RoomMaxBet>(levelOfBets, true, out var roomMaxBet) || !Enum.TryParse<RoomUnitBet>(levelOfBets, true, out var roomUnitBet))
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
                GameType.Baccarat => new BaccaratRoom(roomId, roomName, new BaccaratBetHandler(), maxUsers, (long)roomMinBet, (long)roomMaxBet, (long)roomUnitBet, _hubContext) { RoomName = roomName },
            };
            _storage.Rooms.TryAdd(roomId, room);
            return roomId;
        }

        public async Task<bool> RoomAddUserAsync(int userId, int roomId)
        {
            _logger.LogInformation("RoomAddUserAsync {UserId} {RoomId}", userId, roomId);
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
            _logger.LogInformation("RoomRemoveUserAsync {UserId} {RoomId}", userId, roomId);
            if (!_storage.Rooms.TryGetValue(roomId, out var room))
            {
                // 房间不存在：可能是已经被删除了 -> 清理映射并返回 true（认为移除成功）
                _storage.UserIdToRoomId.TryRemove(userId, out _);
                _logger.LogWarning("Attempted to remove user {UserId} from non-existent room {RoomId}. Cleared mapping.", userId, roomId);
                return true;
            }

            if (!room.Users.TryGetValue(userId, out var userInRoom))
            {
                // 用户不在房间里，仍然清理映射
                _storage.UserIdToRoomId.TryRemove(userId, out _);
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

        public virtual async Task<bool> PlaceBetsAsync(int roomId, int userId, Dictionary<string, long> BetSidesWithAmount)
        {
            _logger.LogInformation("Placing bets for user {UserId} in room {RoomId}", userId, roomId);
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

        public virtual async Task<IUser?> GetUserInRoomAsync(int roomId, int userId)
        {
            _logger.LogInformation("Getting user {UserId} in room {RoomId}", userId, roomId);
            if (!_storage.Rooms.TryGetValue(roomId, out var room))
            {
                throw new RoomNotFoundException(roomId);
            }
            if (!room.Users.TryGetValue(userId, out var userInRoom))
            {
                throw new RoomUserNotFoundException(userId, roomId);
            }
            return userInRoom;
        }

        public virtual async Task<int?> GetRoomIdByUserIdAsync(int userId)
        {
            _logger.LogInformation("Getting room id for user {UserId}", userId);
            if (_storage.UserIdToRoomId.TryGetValue(userId, out var roomId))
            {
                return roomId;
            }
            return null;
        }
    }
}
