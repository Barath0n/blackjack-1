using Blackjack.Core;

namespace Blackjack.App;

public sealed class GameStatistics
{
    public long RoundsPlayed { get; set; }

    public long HandsPlayed { get; set; }

    public long HandsWon { get; set; }

    public long HandsLost { get; set; }

    public long HandsPushed { get; set; }

    public long Blackjacks { get; set; }

    public long Surrenders { get; set; }

    public decimal TotalWagered { get; set; }

    public decimal NetProfit { get; set; }

    public decimal BiggestRoundWin { get; set; }

    public decimal BiggestRoundLoss { get; set; }

    public decimal HighestBankroll { get; set; }

    public decimal WinRate =>
        HandsPlayed == 0
            ? 0m
            : (decimal)HandsWon / HandsPlayed;

    public void RecordRound(
        IEnumerable<RoundOutcome> outcomes,
        decimal totalWagered,
        decimal netResult,
        decimal bankroll)
    {
        RoundOutcome[] outcomeArray =
            outcomes.ToArray();

        RoundsPlayed++;
        HandsPlayed += outcomeArray.Length;

        foreach (RoundOutcome outcome in outcomeArray)
        {
            switch (outcome)
            {
                case RoundOutcome.PlayerBlackjack:
                    HandsWon++;
                    Blackjacks++;
                    break;

                case RoundOutcome.PlayerWin:
                case RoundOutcome.DealerBust:
                    HandsWon++;
                    break;

                case RoundOutcome.DealerWin:
                case RoundOutcome.PlayerBust:
                    HandsLost++;
                    break;

                case RoundOutcome.Push:
                    HandsPushed++;
                    break;

                case RoundOutcome.Surrendered:
                    Surrenders++;
                    break;
            }
        }

        TotalWagered += totalWagered;
        NetProfit += netResult;

        if (netResult > BiggestRoundWin)
        {
            BiggestRoundWin = netResult;
        }

        decimal loss = Math.Abs(
            Math.Min(netResult, 0m));

        if (loss > BiggestRoundLoss)
        {
            BiggestRoundLoss = loss;
        }

        HighestBankroll = Math.Max(
            HighestBankroll,
            bankroll);
    }

    public void Reset(decimal currentBankroll)
    {
        RoundsPlayed = 0;
        HandsPlayed = 0;
        HandsWon = 0;
        HandsLost = 0;
        HandsPushed = 0;
        Blackjacks = 0;
        Surrenders = 0;
        TotalWagered = 0m;
        NetProfit = 0m;
        BiggestRoundWin = 0m;
        BiggestRoundLoss = 0m;
        HighestBankroll = currentBankroll;
    }

    public void Normalize(decimal currentBankroll)
    {
        RoundsPlayed = Math.Max(0, RoundsPlayed);
        HandsPlayed = Math.Max(0, HandsPlayed);
        HandsWon = Math.Max(0, HandsWon);
        HandsLost = Math.Max(0, HandsLost);
        HandsPushed = Math.Max(0, HandsPushed);
        Blackjacks = Math.Max(0, Blackjacks);
        Surrenders = Math.Max(0, Surrenders);
        TotalWagered = Math.Max(0m, TotalWagered);
        BiggestRoundWin = Math.Max(0m, BiggestRoundWin);
        BiggestRoundLoss = Math.Max(0m, BiggestRoundLoss);
        HighestBankroll = Math.Max(
            HighestBankroll,
            currentBankroll);
    }
}
