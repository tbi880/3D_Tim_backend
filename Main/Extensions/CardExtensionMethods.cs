using DeckOfCardsLibrary;

namespace _3D_Tim_backend.Extensions;

public static class CardExtensions
{
    public static int ToBaccaratPoint(this Card.Rank rank)
    {
        switch (rank)
        {
            case Card.Rank.Ten:
            case Card.Rank.Jack:
            case Card.Rank.Queen:
            case Card.Rank.King:
                return 0;
            case Card.Rank.Ace:
                return 1;
            default:
                // Two=2, Three=3, ... Nine=9
                return (int)rank;
        }
    }
}