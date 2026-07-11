using Blackjack.Core;

namespace Blackjack.App;

public sealed class AppSettings
{
    private static readonly int[] SupportedDeckCounts =
        [1, 2, 4, 6, 8];

    public decimal StartingBankroll { get; set; } = 500m;

    public int DeckCount { get; set; } = 1;

    public bool DealerHitsSoft17 { get; set; }

    public decimal LastBet { get; set; } = 25m;

    public BlackjackRules CreateRules() =>
        new(
            DeckCount,
            DealerHitsSoft17);

    public AppSettings Clone() =>
        new()
        {
            StartingBankroll = StartingBankroll,
            DeckCount = DeckCount,
            DealerHitsSoft17 = DealerHitsSoft17,
            LastBet = LastBet
        };

    public void Normalize()
    {
        if (StartingBankroll <= 0)
        {
            StartingBankroll = 500m;
        }

        if (!SupportedDeckCounts.Contains(DeckCount))
        {
            DeckCount = 1;
        }

        if (LastBet <= 0)
        {
            LastBet = Math.Min(
                25m,
                StartingBankroll);
        }
    }
}
