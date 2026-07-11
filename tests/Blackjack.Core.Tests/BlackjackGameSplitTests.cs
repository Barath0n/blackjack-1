using Blackjack.Core;

namespace Blackjack.Core.Tests;

public sealed class BlackjackGameSplitTests
{
    [Fact]
    public void SplitCreatesTwoHandsAndSettlesThemIndependently()
    {
        Card[] drawOrder =
        [
            new(Suit.Hearts, Rank.Eight),
            new(Suit.Clubs, Rank.Ten),
            new(Suit.Spades, Rank.Eight),
            new(Suit.Diamonds, Rank.Seven),
            new(Suit.Clubs, Rank.Three),
            new(Suit.Hearts, Rank.King)
        ];

        BlackjackGame game = new(500m, drawOrder);

        game.StartRound(25m);

        Assert.True(game.CanSplit);

        game.Split();

        Assert.Equal(2, game.PlayerHands.Count);
        Assert.Equal(450m, game.Bankroll);
        Assert.Equal(11, game.PlayerHands[0].Hand.Score);
        Assert.Equal(18, game.PlayerHands[1].Hand.Score);
        Assert.Equal(0, game.ActiveHandIndex);

        game.Stand();

        Assert.Equal(1, game.ActiveHandIndex);

        game.Stand();

        Assert.Equal(RoundState.RoundOver, game.State);
        Assert.Equal(RoundOutcome.DealerWin, game.PlayerHands[0].Outcome);
        Assert.Equal(RoundOutcome.PlayerWin, game.PlayerHands[1].Outcome);
        Assert.Equal(RoundOutcome.Mixed, game.Outcome);
        Assert.Equal(500m, game.Bankroll);
    }

    [Fact]
    public void SplitAcesReceiveOneCardEachAndAreResolvedImmediately()
    {
        Card[] drawOrder =
        [
            new(Suit.Hearts, Rank.Ace),
            new(Suit.Clubs, Rank.Ten),
            new(Suit.Spades, Rank.Ace),
            new(Suit.Diamonds, Rank.Six),
            new(Suit.Clubs, Rank.Nine),
            new(Suit.Hearts, Rank.King),
            new(Suit.Spades, Rank.Two)
        ];

        BlackjackGame game = new(500m, drawOrder);

        game.StartRound(25m);
        game.Split();

        Assert.Equal(RoundState.RoundOver, game.State);
        Assert.All(game.PlayerHands, hand => Assert.True(hand.IsComplete));
        Assert.Equal(20, game.PlayerHands[0].Hand.Score);
        Assert.Equal(21, game.PlayerHands[1].Hand.Score);
        Assert.Equal(RoundOutcome.PlayerWin, game.PlayerHands[0].Outcome);
        Assert.Equal(RoundOutcome.PlayerWin, game.PlayerHands[1].Outcome);
        Assert.Equal(550m, game.Bankroll);
    }
}
