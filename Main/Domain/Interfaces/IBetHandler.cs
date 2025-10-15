namespace _3D_Tim_backend.Domain
{
    public interface IBetHandler
    {
        Task PlaceBetAsync(IUser user, string betSide, long betAmount, long minBet, long maxBet, long unitBet);
        Task ClearBetAsync(IUser user);
        Task HandleResultAsync(IUser user, List<string> winningSides);
    }
}
