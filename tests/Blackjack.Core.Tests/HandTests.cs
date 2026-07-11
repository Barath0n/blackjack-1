using Blackjack.Core;

namespace Blackjack.Core.Tests;

public sealed class HandTests
{
    [Fact]
    public void AceAndKingAreBlackjack()
    {
        Hand hand = new();
        hand.Add(new Card(Suit.Spades, Rank.Ace));
        hand.Add(new Card(Suit.Hearts, Rank.King));

        Assert.Equal(21, hand.Score);
        Assert.True(hand.IsBlackjack);
    }

    [Fact]
    public void AceFallsBackFromElevenToOne()
    {
        Hand hand = new();
        hand.Add(new Card(Suit.Spades, Rank.Ace));
        hand.Add(new Card(Suit.Hearts, Rank.Six));
        hand.Add(new Card(Suit.Clubs, Rank.Eight));

        Assert.Equal(15, hand.Score);
        Assert.False(hand.IsBust);
    }

    [Fact]
    public void TwoAcesAreScoredCorrectly()
    {
        Hand hand = new();
        hand.Add(new Card(Suit.Spades, Rank.Ace));
        hand.Add(new Card(Suit.Hearts, Rank.Ace));
        hand.Add(new Card(Suit.Clubs, Rank.Nine));

        Assert.Equal(21, hand.Score);
    }
}
