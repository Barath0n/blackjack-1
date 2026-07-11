namespace Blackjack.Core;

public sealed record BlackjackRules
{
    private static readonly int[] SupportedDeckCounts =
        [1, 2, 4, 6, 8];

    public BlackjackRules(
        int deckCount = 1,
        bool dealerHitsSoft17 = false)
    {
        if (!SupportedDeckCounts.Contains(deckCount))
        {
            throw new ArgumentOutOfRangeException(
                nameof(deckCount),
                "Deck count must be 1, 2, 4, 6 or 8.");
        }

        DeckCount = deckCount;
        DealerHitsSoft17 = dealerHitsSoft17;
    }

    public int DeckCount { get; }

    public bool DealerHitsSoft17 { get; }

    public string DealerRuleName =>
        DealerHitsSoft17 ? "H17" : "S17";
}
