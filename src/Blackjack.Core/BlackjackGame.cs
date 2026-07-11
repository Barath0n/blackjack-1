namespace Blackjack.Core;

public sealed class BlackjackGame
{
    private readonly List<PlayerHandState> _playerHands = [];
    private readonly Random? _random;
    private readonly int _reshuffleThreshold;
    private Deck _deck;

    public BlackjackGame(
        decimal startingBankroll = 500m,
        Random? random = null,
        BlackjackRules? rules = null)
    {
        ValidateStartingBankroll(startingBankroll);

        Rules = rules ?? new BlackjackRules();
        Bankroll = startingBankroll;
        _random = random;
        _reshuffleThreshold = 15 * Rules.DeckCount;
        _deck = new Deck(Rules.DeckCount, random);
    }

    internal BlackjackGame(
        decimal startingBankroll,
        IEnumerable<Card> drawOrder)
        : this(
            startingBankroll,
            drawOrder,
            new BlackjackRules())
    {
    }

    internal BlackjackGame(
        decimal startingBankroll,
        IEnumerable<Card> drawOrder,
        BlackjackRules rules)
    {
        ValidateStartingBankroll(startingBankroll);
        ArgumentNullException.ThrowIfNull(rules);

        Rules = rules;
        Bankroll = startingBankroll;
        _reshuffleThreshold = 0;
        _deck = new Deck(drawOrder);
    }

    public BlackjackRules Rules { get; }

    public decimal Bankroll { get; private set; }

    public decimal CurrentBet => _playerHands.Sum(hand => hand.Bet);

    public IReadOnlyList<PlayerHandState> PlayerHands => _playerHands;

    public Hand DealerHand { get; } = new();

    public int ActiveHandIndex { get; private set; } = -1;

    public PlayerHandState? ActiveHand =>
        ActiveHandIndex >= 0 && ActiveHandIndex < _playerHands.Count
            ? _playerHands[ActiveHandIndex]
            : null;

    public RoundState State { get; private set; } = RoundState.WaitingForBet;

    public RoundOutcome Outcome { get; private set; } = RoundOutcome.None;

    public bool IsSplitRound => _playerHands.Count > 1;

    public bool IsDealerHoleCardHidden => State == RoundState.PlayerTurn;

    public bool CanHit =>
        State == RoundState.PlayerTurn &&
        ActiveHand is { IsComplete: false };

    public bool CanStand => CanHit;

    public bool CanDoubleDown =>
        CanHit &&
        ActiveHand!.Hand.Cards.Count == 2 &&
        Bankroll >= ActiveHand.Bet;

    public bool CanSplit =>
        CanHit &&
        _playerHands.Count == 1 &&
        ActiveHand!.Hand.CanSplit &&
        Bankroll >= ActiveHand.Bet;

    public bool CanSurrender =>
        CanHit &&
        !IsSplitRound &&
        ActiveHand!.Hand.Cards.Count == 2;

    public void StartRound(decimal bet)
    {
        if (State == RoundState.PlayerTurn)
        {
            throw new InvalidOperationException("The current round is still active.");
        }

        if (bet <= 0 || bet > Bankroll)
        {
            throw new ArgumentOutOfRangeException(
                nameof(bet),
                "Bet must be positive and no higher than the bankroll.");
        }

        if (_deck.RemainingCards < _reshuffleThreshold)
        {
            _deck = new Deck(
                Rules.DeckCount,
                _random);
        }

        _playerHands.Clear();
        DealerHand.Clear();
        Outcome = RoundOutcome.None;
        ActiveHandIndex = 0;

        PlayerHandState playerHand = new(bet);
        _playerHands.Add(playerHand);

        Bankroll -= bet;

        playerHand.Hand.Add(_deck.Draw());
        DealerHand.Add(_deck.Draw());
        playerHand.Hand.Add(_deck.Draw());
        DealerHand.Add(_deck.Draw());

        State = RoundState.PlayerTurn;

        if (playerHand.Hand.IsBlackjack || DealerHand.IsBlackjack)
        {
            ResolveInitialBlackjack();
        }
    }

    public void Hit()
    {
        PlayerHandState hand = GetActiveHand();

        hand.Hand.Add(_deck.Draw());

        if (hand.Hand.IsBust)
        {
            hand.Outcome = RoundOutcome.PlayerBust;
            hand.IsComplete = true;
            AdvanceToNextHandOrDealer();
        }
        else if (hand.Hand.Score == 21)
        {
            hand.IsComplete = true;
            AdvanceToNextHandOrDealer();
        }
    }

    public void Stand()
    {
        PlayerHandState hand = GetActiveHand();

        hand.IsComplete = true;
        AdvanceToNextHandOrDealer();
    }

    public void DoubleDown()
    {
        if (!CanDoubleDown)
        {
            throw new InvalidOperationException("Double down is not available.");
        }

        PlayerHandState hand = GetActiveHand();

        Bankroll -= hand.Bet;
        hand.Bet *= 2;
        hand.Hand.Add(_deck.Draw());

        if (hand.Hand.IsBust)
        {
            hand.Outcome = RoundOutcome.PlayerBust;
        }

        hand.IsComplete = true;
        AdvanceToNextHandOrDealer();
    }

    public void Split()
    {
        if (!CanSplit)
        {
            throw new InvalidOperationException("Split is not available.");
        }

        PlayerHandState originalHand = GetActiveHand();
        decimal splitBet = originalHand.Bet;
        Card firstCard = originalHand.Hand.Cards[0];
        Card secondCard = originalHand.Hand.Cards[1];

        Bankroll -= splitBet;
        _playerHands.Clear();

        PlayerHandState firstHand = new(splitBet, isFromSplit: true);
        firstHand.Hand.Add(firstCard);
        firstHand.Hand.Add(_deck.Draw());

        PlayerHandState secondHand = new(splitBet, isFromSplit: true);
        secondHand.Hand.Add(secondCard);
        secondHand.Hand.Add(_deck.Draw());

        _playerHands.Add(firstHand);
        _playerHands.Add(secondHand);
        ActiveHandIndex = 0;

        bool splitAces =
            firstCard.Rank == Rank.Ace &&
            secondCard.Rank == Rank.Ace;

        if (splitAces)
        {
            firstHand.IsComplete = true;
            secondHand.IsComplete = true;
            ResolveDealerAndHands();
            return;
        }

        if (firstHand.Hand.Score == 21)
        {
            firstHand.IsComplete = true;
        }

        if (secondHand.Hand.Score == 21)
        {
            secondHand.IsComplete = true;
        }

        MoveToFirstIncompleteHandOrDealer();
    }

    public void Surrender()
    {
        if (!CanSurrender)
        {
            throw new InvalidOperationException("Surrender is not available.");
        }

        PlayerHandState hand = GetActiveHand();

        hand.Outcome = RoundOutcome.Surrendered;
        hand.IsComplete = true;
        PayHand(hand);
        FinishRound();
    }

    private static void ValidateStartingBankroll(decimal startingBankroll)
    {
        if (startingBankroll < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(startingBankroll));
        }
    }

    private void ResolveInitialBlackjack()
    {
        PlayerHandState hand = _playerHands[0];

        if (hand.Hand.IsBlackjack && DealerHand.IsBlackjack)
        {
            hand.Outcome = RoundOutcome.Push;
        }
        else if (hand.Hand.IsBlackjack)
        {
            hand.Outcome = RoundOutcome.PlayerBlackjack;
        }
        else
        {
            hand.Outcome = RoundOutcome.DealerWin;
        }

        hand.IsComplete = true;
        PayHand(hand);
        FinishRound();
    }

    private void AdvanceToNextHandOrDealer()
    {
        for (int index = ActiveHandIndex + 1; index < _playerHands.Count; index++)
        {
            if (!_playerHands[index].IsComplete)
            {
                ActiveHandIndex = index;
                return;
            }
        }

        ResolveDealerAndHands();
    }

    private void MoveToFirstIncompleteHandOrDealer()
    {
        int nextHandIndex = _playerHands.FindIndex(hand => !hand.IsComplete);

        if (nextHandIndex >= 0)
        {
            ActiveHandIndex = nextHandIndex;
            return;
        }

        ResolveDealerAndHands();
    }

    private void ResolveDealerAndHands()
    {
        bool dealerNeedsToPlay = _playerHands.Any(hand => !hand.Hand.IsBust);

        if (dealerNeedsToPlay)
        {
            while (DealerShouldHit())
            {
                DealerHand.Add(_deck.Draw());
            }
        }

        foreach (PlayerHandState hand in _playerHands)
        {
            if (hand.Outcome == RoundOutcome.PlayerBust)
            {
                hand.IsComplete = true;
                continue;
            }

            hand.Outcome = GetOutcomeAgainstDealer(hand.Hand);
            hand.IsComplete = true;
            PayHand(hand);
        }

        FinishRound();
    }

    private bool DealerShouldHit() =>
        DealerHand.Score < 17 ||
        (
            Rules.DealerHitsSoft17 &&
            DealerHand.Score == 17 &&
            DealerHand.IsSoft
        );

    private RoundOutcome GetOutcomeAgainstDealer(Hand playerHand)
    {
        if (DealerHand.IsBust)
        {
            return RoundOutcome.DealerBust;
        }

        if (playerHand.Score > DealerHand.Score)
        {
            return RoundOutcome.PlayerWin;
        }

        if (playerHand.Score < DealerHand.Score)
        {
            return RoundOutcome.DealerWin;
        }

        return RoundOutcome.Push;
    }

    private void PayHand(PlayerHandState hand)
    {
        decimal payout = hand.Outcome switch
        {
            RoundOutcome.PlayerBlackjack =>
                hand.Bet * 2.5m,

            RoundOutcome.PlayerWin or
            RoundOutcome.DealerBust =>
                hand.Bet * 2m,

            RoundOutcome.Push =>
                hand.Bet,

            RoundOutcome.Surrendered =>
                hand.Bet * 0.5m,

            _ =>
                0m
        };

        Bankroll += decimal.Round(
            payout,
            2,
            MidpointRounding.AwayFromZero);
    }

    private void FinishRound()
    {
        State = RoundState.RoundOver;
        ActiveHandIndex = -1;

        RoundOutcome[] outcomes = _playerHands
            .Select(hand => hand.Outcome)
            .Distinct()
            .ToArray();

        Outcome = outcomes.Length == 1
            ? outcomes[0]
            : RoundOutcome.Mixed;
    }

    private PlayerHandState GetActiveHand()
    {
        if (State != RoundState.PlayerTurn || ActiveHand is null)
        {
            throw new InvalidOperationException("No player turn is active.");
        }

        return ActiveHand;
    }
}
