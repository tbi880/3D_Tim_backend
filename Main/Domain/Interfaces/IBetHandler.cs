namespace _3D_Tim_backend.Domain
{
    public interface IBetHandler
    {
        Task PlaceBetAsync(IUser user, string betSide, int betAmount, int minBet, int maxBet, int unitBet);
        Task ClearBetAsync(IUser user);
        Task HandleResultAsync(IUser user, List<string> winningSides);
    }
}
