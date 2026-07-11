namespace Blackjack.Core;

public sealed class Deck
{
    private readonly List<Card> _cards;
    private readonly Random _random;

    public Deck(Random? random = null)
    {
        _random = random ?? Random.Shared;
        _cards = CreateStandardCards();
        Shuffle();
    }

    internal Deck(IEnumerable<Card> drawOrder)
    {
        ArgumentNullException.ThrowIfNull(drawOrder);

        _random = Random.Shared;
        _cards = drawOrder.Reverse().ToList();
    }

    public int RemainingCards => _cards.Count;

    public Card Draw()
    {
        if (_cards.Count == 0)
        {
            throw new InvalidOperationException("The deck is empty.");
        }

        Card card = _cards[^1];
        _cards.RemoveAt(_cards.Count - 1);
        return card;
    }

    public void Shuffle()
    {
        for (int index = _cards.Count - 1; index > 0; index--)
        {
            int swapIndex = _random.Next(index + 1);
            (_cards[index], _cards[swapIndex]) = (_cards[swapIndex], _cards[index]);
        }
    }

    private static List<Card> CreateStandardCards() =>
        Enum.GetValues<Suit>()
            .SelectMany(suit => Enum.GetValues<Rank>().Select(rank => new Card(suit, rank)))
            .ToList();
}
