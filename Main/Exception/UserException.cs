namespace _3D_Tim_backend.Exceptions
{
    using System;

    // 用户相关的基础异常类
    public class UserException : Exception
    {
        public UserException(string message) : base(message) { }
    }

    // 用户不存在异常
    public class UserNotFoundException : UserException
    {
        public UserNotFoundException(int userId)
            : base($"User with ID {userId} does not exist.") { }
    }

    public class InsufficientBalanceException : UserException
    {
        public InsufficientBalanceException(int userId, int balance, int requiredBalance)
            : base($"User with ID {userId} has insufficient balance. Current balance: {balance}, required balance: {requiredBalance}") { }
    }
}
