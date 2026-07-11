namespace Blackjack.Core;

public sealed class Hand
{
    private readonly List<Card> _cards = [];

    public IReadOnlyList<Card> Cards => _cards;

    public int Score
    {
        get
        {
            int score = _cards.Sum(card => card.BlackjackValue);
            int aces = _cards.Count(card => card.Rank == Rank.Ace);

            while (score > 21 && aces > 0)
            {
                score -= 10;
                aces--;
            }

            return score;
        }
    }

    public bool IsBlackjack => _cards.Count == 2 && Score == 21;

    public bool IsBust => Score > 21;

    public bool CanSplit =>
        _cards.Count == 2 &&
        _cards[0].BlackjackValue == _cards[1].BlackjackValue;

    public bool IsSoft
    {
        get
        {
            int rawScore = _cards.Sum(card => card.BlackjackValue);
            int adjustedScore = rawScore;
            int aces = _cards.Count(card => card.Rank == Rank.Ace);

            while (adjustedScore > 21 && aces > 0)
            {
                adjustedScore -= 10;
                aces--;
            }

            return aces > 0 && adjustedScore <= 21;
        }
    }

    public void Add(Card card) => _cards.Add(card);

    public void Clear() => _cards.Clear();
}
