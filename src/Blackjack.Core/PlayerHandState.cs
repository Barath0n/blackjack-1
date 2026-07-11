namespace Blackjack.Core;

public sealed class PlayerHandState
{
    internal PlayerHandState(decimal bet, bool isFromSplit = false)
    {
        if (bet <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(bet));
        }

        Bet = bet;
        IsFromSplit = isFromSplit;
    }

    public Hand Hand { get; } = new();

    public decimal Bet { get; internal set; }

    public RoundOutcome Outcome { get; internal set; } = RoundOutcome.None;

    public bool IsComplete { get; internal set; }

    public bool IsFromSplit { get; }
}
