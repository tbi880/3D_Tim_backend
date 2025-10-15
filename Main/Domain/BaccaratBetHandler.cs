namespace _3D_Tim_backend.Domain
{
    using _3D_Tim_backend.Exceptions;
    public class BaccaratBetHandler : IBetHandler
    {

        // 定义赔率表
        private readonly Dictionary<string, double> _oddsTable = new Dictionary<string, double>
        {
            { "SmallTiger", 22 },       // lucky 6 with 5 cards
            { "BigTiger", 50 },    // lucky 6 with 6 cards
            { "TigerTie", 35 },       // tie hand with both 6 points 
            { "Tie", 8 },       // tie
            { "Player", 1},       // player wins
            { "Banker", 0.95 },     // banker wins
            {"Freehand", -1 }       // freehand, skip the round
        };

        public async Task PlaceBetAsync(IUser user, string betSide, long betAmount, long roomMinBet, long roomMaxBet, long roomUnitBet)
        {
            if (betSide == "Freehand")
            {
                await FreehandAsync(user);
                return;
            }
            if (user.MoneyInRoom < betAmount)
            {
                throw new InsufficientBalanceException(user.UserId, user.MoneyInRoom, betAmount);
            }
            if (betSide != "Player" && betSide != "Banker" && betSide != "Tie" && !user.BetSides.ContainsKey("Player") && !user.BetSides.ContainsKey("Banker") && !user.BetSides.ContainsKey("Tie"))
            {
                throw new InvalidSideBetBeforeMainBetException(betSide);
            }
            if (user.BetSides.ContainsKey("freehand"))
            {
                throw new InvalidSideBetAfterFreehandException(betSide);
            }
            if (betAmount < roomMinBet || betAmount > roomMaxBet || betAmount % roomUnitBet != 0)
            {
                throw new InvalidBetAmountException(betAmount, roomMinBet, roomMaxBet, roomUnitBet);
            }

            user.MoneyInRoom -= betAmount;
            user.BetSides.AddOrUpdate(betSide, betAmount, (key, oldValue) => oldValue + betAmount);
        }

        public async Task FreehandAsync(IUser user)
        {
            await ClearBetAsync(user);
            user.BetSides.AddOrUpdate("Freehand", 0, (key, oldValue) => 0);
        }

        public async Task ClearBetAsync(IUser user)
        {
            user.MoneyInRoom += user.BetSides.Values.Sum();
            user.BetSides.Clear();
        }


        public async Task<Dictionary<string, long>> HandleResultAsync(IUser user, List<string> winningSides)
        {
            if (user.BetSides.ContainsKey("Freehand"))
            {
                user.BetSides.Clear();
                return new Dictionary<string, long>();
            }
            long originalTotal = user.MoneyInRoom + user.BetSides.Values.Sum();
            var result = new Dictionary<string, long>();

            if (winningSides.Contains("Tie"))
            {
                user.MoneyInRoom = user.MoneyInRoom + (user.BetSides.TryGetValue("Player", out var PlayerAmount) ? PlayerAmount : 0) + (user.BetSides.TryGetValue("Banker", out var BankerAmount) ? BankerAmount : 0);
            }
            foreach (var bet in user.BetSides)
            {
                var betSide = bet.Key;
                var betAmount = bet.Value;
                if (winningSides.Contains(betSide))
                {
                    var odds = _oddsTable.ContainsKey(betSide) ? _oddsTable[betSide] : 1;
                    long winAmount = (long)(betAmount * (1 + odds));
                    result[betSide] = winAmount;
                    user.MoneyInRoom += winAmount;
                }
            }
            if (originalTotal > user.MoneyInRoom)
            {
                user.LoseCountInRoom++;
            }
            else if (originalTotal == user.MoneyInRoom)
            {
                user.TieCountInRoom++;
            }
            else
            {
                user.WinCountInRoom++;
            }
            user.TotalBetsInRoom++;
            user.BetSides.Clear();
            return result;
        }

    }
}
