namespace _3D_Tim_backend.Exceptions
{
    using System;


    public class RoomException : Exception
    {
        public RoomException(string message) : base(message) { }
    }

    public class RoomNotFoundException : RoomException
    {
        public RoomNotFoundException(int roomId) : base($"Room with ID {roomId} not found.") { }
    }

    public class RoomFullException : RoomException
    {
        public RoomFullException() : base("The room is full.") { }
    }

    public class RoomAlreadyExistsException : RoomException
    {
        public RoomAlreadyExistsException(int roomId) : base($"Room with ID {roomId} already exists.") { }
    }

    public class RoomAlreadyHasTheUserException : RoomException
    {
        public RoomAlreadyHasTheUserException(int userId)
            : base($"User with ID {userId} is already in the room.") { }
    }

    public class RoomEnterInsufficientBalanceException : RoomException
    {
        public RoomEnterInsufficientBalanceException(int userId, int balance, int requiredBalance)
            : base($"User with ID {userId} has insufficient balance. Current balance: {balance}, Minimum required: {requiredBalance}.") { }
    }

    public class RoomSyncUserDataToDbException : RoomException
    {
        public RoomSyncUserDataToDbException(int userId)
            : base($"User with ID {userId} can't be sync to DB") { }
    }

    public class RoomUserNotFoundException : RoomException
    {
        public RoomUserNotFoundException(int userId, int roomId)
            : base($"User with ID {userId} not found in the room {roomId}.") { }
    }

    public class NoCardInShoeException : RoomException
    {
        public NoCardInShoeException() : base("No card in the shoe.") { }
    }

    public class LevelOfBetsNotValidException : RoomException
    {
        public LevelOfBetsNotValidException(string levelOfBets) : base($"Level of bets {levelOfBets} is not valid.") { }
    }

    public class InvalidSideBetBeforeMainBetException : RoomException
    {
        public InvalidSideBetBeforeMainBetException(string betSide) : base($"Invalid side bet {betSide} before main bet.") { }
    }

    public class InvalidSideBetAfterFreehandException : RoomException
    {
        public InvalidSideBetAfterFreehandException(string betSide) : base($"Invalid side bet {betSide} after freehand.") { }
    }

    public class InvalidBetAmountException : RoomException
    {
        public InvalidBetAmountException(int betAmount, int roomMinBet, int roomMaxBet, int roomUnitBet)
            : base($"Invalid bet amount {betAmount}. Minimum: {roomMinBet}, Maximum: {roomMaxBet}, Unit: {roomUnitBet}.") { }
    }

    public class GameTypeNotValidException : RoomException
    {
        public GameTypeNotValidException(string gameType) : base($"Game type {gameType} is not valid.") { }
    }

    public class BaccaratHandsNotAvailableYetException : RoomException
    {
        public BaccaratHandsNotAvailableYetException() : base("Baccarat hands are not available yet.") { }
    }

    public class BaccaratWinningSidesNotAvailableYetException : RoomException
    {
        public BaccaratWinningSidesNotAvailableYetException() : base("Baccarat winning sides are not available yet.") { }
    }
}
