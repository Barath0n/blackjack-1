using Blackjack.Core;

namespace Blackjack.Core.Tests;

public sealed class BlackjackGameConfigurableRulesTests
{
    [Theory]
    [InlineData(1, 52)]
    [InlineData(2, 104)]
    [InlineData(4, 208)]
    [InlineData(6, 312)]
    [InlineData(8, 416)]
    public void ShoeContainsExpectedNumberOfCards(
        int deckCount,
        int expectedCards)
    {
        Deck deck = new(
            deckCount,
            new Random(1234));

        Assert.Equal(
            expectedCards,
            deck.RemainingCards);
    }

    [Fact]
    public void UnsupportedDeckCountIsRejected()
    {
        Assert.Throws<ArgumentOutOfRangeException>(
            () => new BlackjackRules(
                deckCount: 3));
    }

    [Fact]
    public void DefaultRulesStandOnSoftSeventeen()
    {
        BlackjackGame game = CreateGame(
            new BlackjackRules(),
            new(Suit.Spades, Rank.Ten),
            new(Suit.Clubs, Rank.Ace),
            new(Suit.Hearts, Rank.Eight),
            new(Suit.Diamonds, Rank.Six),
            new(Suit.Clubs, Rank.Four));

        game.StartRound(10m);
        game.Stand();

        Assert.Equal(
            2,
            game.DealerHand.Cards.Count);

        Assert.Equal(
            17,
            game.DealerHand.Score);

        Assert.Equal(
            RoundOutcome.PlayerWin,
            game.Outcome);

        Assert.Equal(
            110m,
            game.Bankroll);
    }

    [Fact]
    public void H17RulesHitSoftSeventeen()
    {
        BlackjackRules rules = new(
            dealerHitsSoft17: true);

        BlackjackGame game = CreateGame(
            rules,
            new(Suit.Spades, Rank.Ten),
            new(Suit.Clubs, Rank.Ace),
            new(Suit.Hearts, Rank.Eight),
            new(Suit.Diamonds, Rank.Six),
            new(Suit.Clubs, Rank.Four));

        game.StartRound(10m);
        game.Stand();

        Assert.Equal(
            3,
            game.DealerHand.Cards.Count);

        Assert.Equal(
            21,
            game.DealerHand.Score);

        Assert.Equal(
            RoundOutcome.DealerWin,
            game.Outcome);

        Assert.Equal(
            90m,
            game.Bankroll);
    }

    [Fact]
    public void GameExposesConfiguredRules()
    {
        BlackjackRules rules = new(
            deckCount: 6,
            dealerHitsSoft17: true);

        BlackjackGame game = new(
            startingBankroll: 500m,
            random: new Random(42),
            rules: rules);

        Assert.Equal(
            6,
            game.Rules.DeckCount);

        Assert.True(
            game.Rules.DealerHitsSoft17);

        Assert.Equal(
            "H17",
            game.Rules.DealerRuleName);
    }

    [Fact]
    public void ZeroBankrollCanRepresentABankruptSession()
    {
        BlackjackGame game = new(
            startingBankroll: 0m);

        Assert.Equal(
            0m,
            game.Bankroll);

        Assert.Throws<ArgumentOutOfRangeException>(
            () => game.StartRound(1m));
    }

    [Fact]
    public void FractionalBlackjackPayoutIsRoundedToCents()
    {
        BlackjackGame game = CreateGame(
            new BlackjackRules(),
            new(Suit.Spades, Rank.Ace),
            new(Suit.Clubs, Rank.Nine),
            new(Suit.Hearts, Rank.King),
            new(Suit.Diamonds, Rank.Seven));

        game.StartRound(0.01m);

        Assert.Equal(
            RoundOutcome.PlayerBlackjack,
            game.Outcome);

        Assert.Equal(
            100.02m,
            game.Bankroll);
    }

    private static BlackjackGame CreateGame(
        BlackjackRules rules,
        params Card[] drawOrder) =>
        new(
            100m,
            drawOrder,
            rules);
}
