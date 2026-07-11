using Blackjack.Core;

namespace Blackjack.Core.Tests;

public sealed class BlackjackGameRuleAuditTests
{
    [Fact]
    public void NaturalBlackjackPaysThreeToTwo()
    {
        BlackjackGame game = CreateGame(
            new(Suit.Spades, Rank.Ace),
            new(Suit.Clubs, Rank.Nine),
            new(Suit.Hearts, Rank.King),
            new(Suit.Diamonds, Rank.Seven));

        game.StartRound(20m);

        Assert.Equal(RoundState.RoundOver, game.State);
        Assert.Equal(RoundOutcome.PlayerBlackjack, game.Outcome);
        Assert.Equal(130m, game.Bankroll);
    }

    [Fact]
    public void TwoNaturalBlackjacksPush()
    {
        BlackjackGame game = CreateGame(
            new(Suit.Spades, Rank.Ace),
            new(Suit.Clubs, Rank.Ace),
            new(Suit.Hearts, Rank.King),
            new(Suit.Diamonds, Rank.Queen));

        game.StartRound(20m);

        Assert.Equal(RoundOutcome.Push, game.Outcome);
        Assert.Equal(100m, game.Bankroll);
    }

    [Fact]
    public void DealerBlackjackEndsRoundBeforePlayerActions()
    {
        BlackjackGame game = CreateGame(
            new(Suit.Spades, Rank.Ten),
            new(Suit.Clubs, Rank.Ace),
            new(Suit.Hearts, Rank.Nine),
            new(Suit.Diamonds, Rank.King));

        game.StartRound(20m);

        Assert.Equal(RoundState.RoundOver, game.State);
        Assert.Equal(RoundOutcome.DealerWin, game.Outcome);
        Assert.False(game.CanSurrender);
        Assert.Equal(80m, game.Bankroll);
    }

    [Fact]
    public void DealerStandsOnSoftSeventeen()
    {
        BlackjackGame game = CreateGame(
            new(Suit.Spades, Rank.Ten),
            new(Suit.Clubs, Rank.Ace),
            new(Suit.Hearts, Rank.Eight),
            new(Suit.Diamonds, Rank.Six),
            new(Suit.Clubs, Rank.Four));

        game.StartRound(10m);
        game.Stand();

        Assert.Equal(17, game.DealerHand.Score);
        Assert.True(game.DealerHand.IsSoft);
        Assert.Equal(2, game.DealerHand.Cards.Count);
        Assert.Equal(RoundOutcome.PlayerWin, game.Outcome);
        Assert.Equal(110m, game.Bankroll);
    }

    [Fact]
    public void DealerDrawsOnHardSixteen()
    {
        BlackjackGame game = CreateGame(
            new(Suit.Spades, Rank.Ten),
            new(Suit.Clubs, Rank.Nine),
            new(Suit.Hearts, Rank.Eight),
            new(Suit.Diamonds, Rank.Seven),
            new(Suit.Clubs, Rank.Five));

        game.StartRound(10m);
        game.Stand();

        Assert.Equal(21, game.DealerHand.Score);
        Assert.Equal(3, game.DealerHand.Cards.Count);
        Assert.Equal(RoundOutcome.DealerWin, game.Outcome);
        Assert.Equal(90m, game.Bankroll);
    }

    [Fact]
    public void DoubleDownDoublesStakeAndDrawsExactlyOneCard()
    {
        BlackjackGame game = CreateGame(
            new(Suit.Spades, Rank.Five),
            new(Suit.Clubs, Rank.Six),
            new(Suit.Hearts, Rank.Six),
            new(Suit.Diamonds, Rank.Ten),
            new(Suit.Clubs, Rank.King),
            new(Suit.Hearts, Rank.Two));

        game.StartRound(10m);
        game.DoubleDown();

        PlayerHandState hand = game.PlayerHands[0];

        Assert.Equal(20m, hand.Bet);
        Assert.Equal(3, hand.Hand.Cards.Count);
        Assert.Equal(21, hand.Hand.Score);
        Assert.Equal(RoundOutcome.PlayerWin, hand.Outcome);
        Assert.Equal(120m, game.Bankroll);
    }

    [Fact]
    public void SurrenderReturnsHalfTheStake()
    {
        BlackjackGame game = CreateGame(
            new(Suit.Spades, Rank.Ten),
            new(Suit.Clubs, Rank.Nine),
            new(Suit.Hearts, Rank.Six),
            new(Suit.Diamonds, Rank.Seven));

        game.StartRound(20m);
        game.Surrender();

        Assert.Equal(RoundOutcome.Surrendered, game.Outcome);
        Assert.Equal(90m, game.Bankroll);
    }

    [Fact]
    public void SurrenderIsUnavailableAfterHit()
    {
        BlackjackGame game = CreateGame(
            new(Suit.Spades, Rank.Ten),
            new(Suit.Clubs, Rank.Nine),
            new(Suit.Hearts, Rank.Four),
            new(Suit.Diamonds, Rank.Seven),
            new(Suit.Clubs, Rank.Two));

        game.StartRound(10m);
        game.Hit();

        Assert.False(game.CanSurrender);
    }

    [Fact]
    public void MixedTenValueCardsMayBeSplit()
    {
        BlackjackGame game = CreateGame(
            new(Suit.Spades, Rank.Ten),
            new(Suit.Clubs, Rank.Six),
            new(Suit.Hearts, Rank.King),
            new(Suit.Diamonds, Rank.Nine));

        game.StartRound(10m);

        Assert.True(game.CanSplit);
    }

    [Fact]
    public void DoubleAfterSplitIsAllowed()
    {
        BlackjackGame game = CreateGame(
            new(Suit.Spades, Rank.Eight),
            new(Suit.Clubs, Rank.Ten),
            new(Suit.Hearts, Rank.Eight),
            new(Suit.Diamonds, Rank.Seven),
            new(Suit.Clubs, Rank.Three),
            new(Suit.Hearts, Rank.Two),
            new(Suit.Spades, Rank.King));

        game.StartRound(10m);
        game.Split();

        Assert.True(game.CanDoubleDown);

        game.DoubleDown();

        Assert.Equal(20m, game.PlayerHands[0].Bet);
        Assert.Equal(3, game.PlayerHands[0].Hand.Cards.Count);
        Assert.Equal(1, game.ActiveHandIndex);
        Assert.Equal(70m, game.Bankroll);
    }

    [Fact]
    public void SurrenderIsUnavailableAfterSplit()
    {
        BlackjackGame game = CreateGame(
            new(Suit.Spades, Rank.Eight),
            new(Suit.Clubs, Rank.Ten),
            new(Suit.Hearts, Rank.Eight),
            new(Suit.Diamonds, Rank.Seven),
            new(Suit.Clubs, Rank.Three),
            new(Suit.Hearts, Rank.Two));

        game.StartRound(10m);
        game.Split();

        Assert.False(game.CanSurrender);
    }

    [Fact]
    public void ResplittingIsCurrentlyUnavailable()
    {
        BlackjackGame game = CreateGame(
            new(Suit.Spades, Rank.Eight),
            new(Suit.Clubs, Rank.Ten),
            new(Suit.Hearts, Rank.Eight),
            new(Suit.Diamonds, Rank.Seven),
            new(Suit.Clubs, Rank.Eight),
            new(Suit.Hearts, Rank.Eight));

        game.StartRound(10m);
        game.Split();

        Assert.True(game.PlayerHands[0].Hand.CanSplit);
        Assert.False(game.CanSplit);
    }

    [Fact]
    public void TwentyOneAfterSplittingAcesPaysAsNormalWin()
    {
        BlackjackGame game = CreateGame(
            new(Suit.Spades, Rank.Ace),
            new(Suit.Clubs, Rank.Ten),
            new(Suit.Hearts, Rank.Ace),
            new(Suit.Diamonds, Rank.Seven),
            new(Suit.Clubs, Rank.King),
            new(Suit.Hearts, Rank.Nine));

        game.StartRound(10m);
        game.Split();

        Assert.Equal(RoundState.RoundOver, game.State);
        Assert.Equal(21, game.PlayerHands[0].Hand.Score);
        Assert.Equal(RoundOutcome.PlayerWin, game.PlayerHands[0].Outcome);
        Assert.Equal(RoundOutcome.PlayerWin, game.PlayerHands[1].Outcome);
        Assert.Equal(120m, game.Bankroll);
    }

    [Fact]
    public void DealerBustPaysEveryNonBustSplitHand()
    {
        BlackjackGame game = CreateGame(
            new(Suit.Spades, Rank.Eight),
            new(Suit.Clubs, Rank.Six),
            new(Suit.Hearts, Rank.Eight),
            new(Suit.Diamonds, Rank.Ten),
            new(Suit.Clubs, Rank.Two),
            new(Suit.Hearts, Rank.Three),
            new(Suit.Spades, Rank.King));

        game.StartRound(10m);
        game.Split();
        game.Stand();
        game.Stand();

        Assert.True(game.DealerHand.IsBust);
        Assert.All(
            game.PlayerHands,
            hand => Assert.Equal(RoundOutcome.DealerBust, hand.Outcome));
        Assert.Equal(120m, game.Bankroll);
    }

    [Fact]
    public void PushReturnsTheStake()
    {
        BlackjackGame game = CreateGame(
            new(Suit.Spades, Rank.Ten),
            new(Suit.Clubs, Rank.Nine),
            new(Suit.Hearts, Rank.Eight),
            new(Suit.Diamonds, Rank.Nine));

        game.StartRound(10m);
        game.Stand();

        Assert.Equal(RoundOutcome.Push, game.Outcome);
        Assert.Equal(100m, game.Bankroll);
    }

    [Fact]
    public void DealerDoesNotDrawWhenEveryPlayerHandHasBusted()
    {
        BlackjackGame game = CreateGame(
            new(Suit.Spades, Rank.Ten),
            new(Suit.Clubs, Rank.Five),
            new(Suit.Hearts, Rank.Nine),
            new(Suit.Diamonds, Rank.Six),
            new(Suit.Clubs, Rank.Five),
            new(Suit.Hearts, Rank.King));

        game.StartRound(10m);
        game.Hit();

        Assert.Equal(RoundOutcome.PlayerBust, game.Outcome);
        Assert.Equal(2, game.DealerHand.Cards.Count);
        Assert.Equal(11, game.DealerHand.Score);
        Assert.Equal(90m, game.Bankroll);
    }

    private static BlackjackGame CreateGame(params Card[] drawOrder) =>
        new(100m, drawOrder);
}
