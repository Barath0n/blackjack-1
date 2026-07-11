namespace Blackjack.Core;

public sealed record Card(Suit Suit, Rank Rank)
{
    public int BlackjackValue => Rank switch
    {
        Rank.Jack or Rank.Queen or Rank.King => 10,
        Rank.Ace => 11,
        _ => (int)Rank
    };

    public string DisplayName => $"{RankSymbol}{SuitSymbol}";

    private string RankSymbol => Rank switch
    {
        Rank.Jack => "J",
        Rank.Queen => "Q",
        Rank.King => "K",
        Rank.Ace => "A",
        _ => ((int)Rank).ToString()
    };

    private string SuitSymbol => Suit switch
    {
        Suit.Clubs => "♣",
        Suit.Diamonds => "♦",
        Suit.Hearts => "♥",
        Suit.Spades => "♠",
        _ => throw new ArgumentOutOfRangeException()
    };
}
