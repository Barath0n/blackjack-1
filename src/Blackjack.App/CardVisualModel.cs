using System.Windows;
using System.Windows.Media;
using Blackjack.Core;

namespace Blackjack.App;

public sealed class CardVisualModel
{
    private CardVisualModel(
        string rank,
        string suit,
        bool isRed,
        bool isFaceDown)
    {
        Rank = rank;
        Suit = suit;
        IsFaceDown = isFaceDown;
        Foreground = isRed
            ? new SolidColorBrush(Color.FromRgb(190, 35, 47))
            : new SolidColorBrush(Color.FromRgb(24, 24, 24));

        FrontVisibility = isFaceDown
            ? Visibility.Collapsed
            : Visibility.Visible;

        BackVisibility = isFaceDown
            ? Visibility.Visible
            : Visibility.Collapsed;
    }

    public string Rank { get; }

    public string Suit { get; }

    public Brush Foreground { get; }

    public bool IsFaceDown { get; }

    public Visibility FrontVisibility { get; }

    public Visibility BackVisibility { get; }

    public static CardVisualModel FaceDown { get; } =
        new(string.Empty, string.Empty, isRed: false, isFaceDown: true);

    public static CardVisualModel FromCard(Card card)
    {
        ArgumentNullException.ThrowIfNull(card);

        string rank = card.Rank switch
        {
            Blackjack.Core.Rank.Jack => "J",
            Blackjack.Core.Rank.Queen => "Q",
            Blackjack.Core.Rank.King => "K",
            Blackjack.Core.Rank.Ace => "A",
            _ => ((int)card.Rank).ToString()
        };

        string suit = card.Suit switch
        {
            Blackjack.Core.Suit.Clubs => "♣",
            Blackjack.Core.Suit.Diamonds => "♦",
            Blackjack.Core.Suit.Hearts => "♥",
            Blackjack.Core.Suit.Spades => "♠",
            _ => throw new ArgumentOutOfRangeException(nameof(card))
        };

        bool isRed = card.Suit is
            Blackjack.Core.Suit.Diamonds or
            Blackjack.Core.Suit.Hearts;

        return new CardVisualModel(rank, suit, isRed, isFaceDown: false);
    }
}
