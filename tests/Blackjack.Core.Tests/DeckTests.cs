using Blackjack.Core;

namespace Blackjack.Core.Tests;

public sealed class DeckTests
{
    [Fact]
    public void StandardDeckContainsFiftyTwoUniqueCards()
    {
        Deck deck = new(new Random(42));
        HashSet<Card> cards = [];

        while (deck.RemainingCards > 0)
        {
            cards.Add(deck.Draw());
        }

        Assert.Equal(52, cards.Count);
    }
}
