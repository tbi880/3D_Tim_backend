namespace _3D_Tim_backend.Domain
{
    using _3D_Tim_backend.Enums;
    using _3D_Tim_backend.Repositories;
    using _3D_Tim_backend.Exceptions;
    using System.Threading.Tasks;
    using DeckOfCardsLibrary;
    using System.Collections.Concurrent;
    using System.Security.Cryptography;
    using _3D_Tim_backend.Extensions;

    public class BaccaratRoom : Room
    {
        private ConcurrentQueue<Card> _shoeOfCards = new();
        private List<string> _resultList = new();
        private bool is_last_hand = false;
        public List<string> BankerHands = [];
        public List<string> PlayerHands = [];
        private List<string> _winningSides = [];
        private CancellationTokenSource _gameLoopCancellationTokenSource;
        private Task _gameLoopTask;

        public BaccaratRoom(int roomId, string roomName, IBetHandler betHandler, int maxUsers, int roomMinBet, int roomMaxBet, int roomUnitBet)
            : base(roomId, roomName, betHandler, maxUsers, roomMinBet, roomMaxBet, roomUnitBet)
        {
            GameType = GameType.Baccarat;
            GetAndShuffleNewDecks();
            _resultList = new List<string>();
            StartGameLoop();
        }

        public override void StartGameLoop()
        {
            if (_gameLoopTask == null || _gameLoopTask.IsCompleted)
            {
                _gameLoopCancellationTokenSource = new CancellationTokenSource();
                _gameLoopTask = Task.Run(() => RunGameLoopAsync(_gameLoopCancellationTokenSource.Token));
            }
        }

        public override async Task StopGameLoopAsync()
        {
            if (_gameLoopCancellationTokenSource != null)
            {
                _gameLoopCancellationTokenSource.Cancel();
                try
                {
                    await _gameLoopTask;
                }
                catch (OperationCanceledException)
                {
                    // Ignore
                }
            }
        }

        public override async Task RunGameLoopAsync(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                if (Users.Count > 0)
                {
                    int countdownMilliseconds = 31000; // 1 second for the internet delay
                    int interval = 500;
                    int elapsed = 0;
                    bool lastHandFinished = false;
                    while (elapsed < countdownMilliseconds && !cancellationToken.IsCancellationRequested)
                    {
                        if (is_last_hand && !lastHandFinished)
                        {
                            // NotifyTheLastHand();
                            lastHandFinished = true;
                        }
                        if (AllUsersHaveBet())
                        {
                            break;
                        }
                        await Task.Delay(interval, cancellationToken);
                        elapsed += interval;
                    }

                    foreach (var user in Users.Values)
                    {
                        if (user.BetSides == null || user.BetSides.IsEmpty)
                        {
                            await PlaceBetAsync(user.UserId, "Freehand", 0);
                        }
                    }
                    var winningSides = await StartGameAsync();
                    await HandleResultAsync(winningSides);
                    if (is_last_hand && lastHandFinished)
                    {
                        GetAndShuffleNewDecks();
                        _resultList = new List<string>();
                    }
                }
                else
                {
                    await Task.Delay(1000, cancellationToken);
                }
            }
        }

        private bool AllUsersHaveBet()
        {
            return Users.Values.All(user => user.BetSides != null && user.BetSides.Count > 0);
        }



        public void GetAndShuffleNewDecks()
        {
            is_last_hand = false;
            _shoeOfCards = new ConcurrentQueue<Card>();
            var allCards = new List<Card>();

            for (int i = 0; i < 8; i++)
            {
                var deck = Deck.get();
                deck.shuffle(random: new Random(915));
                Card card;
                while ((card = deck.draw()) != null)
                {
                    allCards.Add(card);
                }
            }
            if (allCards.Count != 416)
            {
                throw new Exception("The number of cards in the deck is not 416.");
            }

            int n = allCards.Count;
            for (int k = 0; k < 8; k++)
            {
                for (int i = 0; i < n - 1; i++)
                {
                    int j = RandomNumberGenerator.GetInt32(i, n);
                    var temp = allCards[i];
                    allCards[i] = allCards[j];
                    allCards[j] = temp;
                }
                k++;
            }
            foreach (var card in allCards)
            {
                _shoeOfCards.Enqueue(card);
            }
        }

        public override async Task PlaceBetAsync(int userId, string betSide, long betAmount)
        {
            if (!Users.TryGetValue(userId, out var user))
            {
                throw new RoomUserNotFoundException(userId, RoomId);
            }
            await _betHandler.PlaceBetAsync(user, betSide, betAmount, RoomMinBet, RoomMaxBet, RoomUnitBet);
        }

        public override async Task ClearBetAsync(int userId)
        {
            if (!Users.TryGetValue(userId, out var user))
            {
                throw new RoomUserNotFoundException(userId, RoomId);
            }
            await _betHandler.ClearBetAsync(user);
        }

        public async Task<Card> DealOneCardAsync()
        {
            if (!is_last_hand && _shoeOfCards.Count == 12)
            {
                is_last_hand = true;
            }
            if (!_shoeOfCards.TryDequeue(out var card))
            {
                throw new NoCardInShoeException();
            }
            return card;
        }

        public override async Task<List<string>> StartGameAsync()
        {
            BankerHands = new List<string>();
            PlayerHands = new List<string>();
            _winningSides = new List<string>();
            // if (is_last_hand)
            // {
            //     NotifyTheLasthand();
            // }   
            var playerCard1 = await DealOneCardAsync();
            var bankerCard1 = await DealOneCardAsync();
            var playerCard2 = await DealOneCardAsync();
            var bankerCard2 = await DealOneCardAsync();
            var playerPoints = playerCard1.rank.ToBaccaratPoint() + playerCard2.rank.ToBaccaratPoint();
            var bankerPoints = bankerCard1.rank.ToBaccaratPoint() + bankerCard2.rank.ToBaccaratPoint();
            playerPoints %= 10;
            bankerPoints %= 10;
            BankerHands.Add(bankerCard1.getDisplayString());
            BankerHands.Add(bankerCard2.getDisplayString());
            PlayerHands.Add(playerCard1.getDisplayString());
            PlayerHands.Add(playerCard2.getDisplayString());
            List<string> winningSides;
            if (playerPoints == 8 || playerPoints == 9 || bankerPoints == 8 || bankerPoints == 9)
            {
                winningSides = await GetWinningSidesAsync(bankerPoints, playerPoints, 4);
                return winningSides;
            }
            if ((playerPoints == 6 && bankerPoints == 7) || (bankerPoints == 6 && playerPoints == 7))
            {
                winningSides = await GetWinningSidesAsync(bankerPoints, playerPoints, 4);
                return winningSides;
            }
            if (playerPoints <= 5)
            {
                var playerCard3 = await DealOneCardAsync();
                playerPoints += playerCard3.rank.ToBaccaratPoint();
                playerPoints %= 10;
                PlayerHands.Add(playerCard3.getDisplayString());
                if (bankerPoints < 3 || (bankerPoints == 3 && playerCard3.rank.ToBaccaratPoint() != 8) || (bankerPoints == 4 && playerCard3.rank.ToBaccaratPoint() >= 2 && playerCard3.rank.ToBaccaratPoint() <= 7) || (bankerPoints == 5 && playerCard3.rank.ToBaccaratPoint() >= 4 && playerCard3.rank.ToBaccaratPoint() <= 7) || (bankerPoints == 6 && playerCard3.rank.ToBaccaratPoint() >= 6 && playerCard3.rank.ToBaccaratPoint() <= 7))
                {
                    var bankerCard3 = await DealOneCardAsync();
                    bankerPoints += bankerCard3.rank.ToBaccaratPoint();
                    bankerPoints %= 10;
                    BankerHands.Add(bankerCard3.getDisplayString());
                    winningSides = await GetWinningSidesAsync(bankerPoints, playerPoints, 6);
                    return winningSides;
                }
                winningSides = await GetWinningSidesAsync(bankerPoints, playerPoints, 5);
                return winningSides;
            }
            if (bankerPoints <= 5)
            {
                var bankerCard3 = await DealOneCardAsync();
                bankerPoints += bankerCard3.rank.ToBaccaratPoint();
                bankerPoints %= 10;
                BankerHands.Add(bankerCard3.getDisplayString());
                winningSides = await GetWinningSidesAsync(bankerPoints, playerPoints, 5);
                return winningSides;
            }
            winningSides = await GetWinningSidesAsync(bankerPoints, playerPoints, 4);
            return winningSides;

        }

        public async Task<List<string>> GetWinningSidesAsync(int bankerPoints, int playerPoints, int totalCardsDrawn)
        {
            _winningSides = new List<string>();
            List<string> winningSides = new List<string>();
            if (playerPoints == bankerPoints)
            {
                winningSides.Add("Tie");
                _resultList.Add("Tie");
                if (bankerPoints == 6)
                {
                    winningSides.Add("TigerTie");
                }
                _winningSides = winningSides;
                return winningSides;
            }
            if (playerPoints > bankerPoints)
            {
                winningSides.Add("Player");
                _resultList.Add("Player");
                _winningSides = winningSides;
                return winningSides;
            }
            if (playerPoints < bankerPoints)
            {
                winningSides.Add("Banker");
                _resultList.Add("Banker");
                if (bankerPoints == 6)
                {
                    if (totalCardsDrawn == 6)
                    {
                        winningSides.Add("BigTiger");
                    }
                    else if (totalCardsDrawn == 5)
                    {
                        winningSides.Add("SmallTiger");
                    }
                }
            }
            _winningSides = winningSides;
            return winningSides;
        }


        public async Task HandleResultAsync(List<string> winningSides)
        {
            foreach (var user in Users.Values)
            {
                await _betHandler.HandleResultAsync(user, winningSides);
            }
        }

        public async Task<List<List<string>>> GetLatestGameHandsAsync()
        {
            if (_winningSides.Count == 0)
            {
                throw new BaccaratHandsNotAvailableYetException();
            }
            return new List<List<string>> { PlayerHands, BankerHands };
        }

        public async Task<List<string>> GetResultListAsync()
        {
            return _resultList;
        }

        public async Task<List<string>> GetLatestGameWinningSidesAsync()
        {
            if (_winningSides.Count == 0)
            {
                throw new BaccaratWinningSidesNotAvailableYetException();
            }
            return _winningSides;
        }




    }
}
