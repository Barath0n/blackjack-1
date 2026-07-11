namespace Blackjack.Core;

public sealed class BlackjackGame
{
    private Deck _deck;

    public BlackjackGame(decimal startingBankroll = 500m, Random? random = null)
    {
        if (startingBankroll <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(startingBankroll));
        }

        Bankroll = startingBankroll;
        _deck = new Deck(random);
    }

    public decimal Bankroll { get; private set; }

    public decimal CurrentBet { get; private set; }

    public Hand PlayerHand { get; } = new();

    public Hand DealerHand { get; } = new();

    public RoundState State { get; private set; } = RoundState.WaitingForBet;

    public RoundOutcome Outcome { get; private set; } = RoundOutcome.None;

    public bool IsDealerHoleCardHidden => State == RoundState.PlayerTurn;

    public bool CanHit => State == RoundState.PlayerTurn;

    public bool CanStand => State == RoundState.PlayerTurn;

    public bool CanDoubleDown =>
        State == RoundState.PlayerTurn &&
        PlayerHand.Cards.Count == 2 &&
        Bankroll >= CurrentBet;

    public bool CanSurrender =>
        State == RoundState.PlayerTurn &&
        PlayerHand.Cards.Count == 2;

    public void StartRound(decimal bet)
    {
        if (State == RoundState.PlayerTurn)
        {
            throw new InvalidOperationException("The current round is still active.");
        }

        if (bet <= 0 || bet > Bankroll)
        {
            throw new ArgumentOutOfRangeException(nameof(bet), "Bet must be positive and no higher than the bankroll.");
        }

        if (_deck.RemainingCards < 15)
        {
            _deck = new Deck();
        }

        PlayerHand.Clear();
        DealerHand.Clear();
        Outcome = RoundOutcome.None;

        CurrentBet = bet;
        Bankroll -= bet;

        PlayerHand.Add(_deck.Draw());
        DealerHand.Add(_deck.Draw());
        PlayerHand.Add(_deck.Draw());
        DealerHand.Add(_deck.Draw());

        if (PlayerHand.IsBlackjack || DealerHand.IsBlackjack)
        {
            ResolveInitialBlackjack();
            return;
        }

        State = RoundState.PlayerTurn;
    }

    public void Hit()
    {
        EnsurePlayerTurn();

        PlayerHand.Add(_deck.Draw());

        if (PlayerHand.IsBust)
        {
            CompleteRound(RoundOutcome.PlayerBust);
        }
        else if (PlayerHand.Score == 21)
        {
            Stand();
        }
    }

    public void Stand()
    {
        EnsurePlayerTurn();

        while (DealerHand.Score < 17)
        {
            DealerHand.Add(_deck.Draw());
        }

        if (DealerHand.IsBust)
        {
            CompleteRound(RoundOutcome.DealerBust);
        }
        else if (PlayerHand.Score > DealerHand.Score)
        {
            CompleteRound(RoundOutcome.PlayerWin);
        }
        else if (PlayerHand.Score < DealerHand.Score)
        {
            CompleteRound(RoundOutcome.DealerWin);
        }
        else
        {
            CompleteRound(RoundOutcome.Push);
        }
    }

    public void DoubleDown()
    {
        if (!CanDoubleDown)
        {
            throw new InvalidOperationException("Double down is not available.");
        }

        Bankroll -= CurrentBet;
        CurrentBet *= 2;

        PlayerHand.Add(_deck.Draw());

        if (PlayerHand.IsBust)
        {
            CompleteRound(RoundOutcome.PlayerBust);
            return;
        }

        Stand();
    }

    public void Surrender()
    {
        if (!CanSurrender)
        {
            throw new InvalidOperationException("Surrender is not available.");
        }

        CompleteRound(RoundOutcome.Surrendered);
    }

    private void ResolveInitialBlackjack()
    {
        if (PlayerHand.IsBlackjack && DealerHand.IsBlackjack)
        {
            CompleteRound(RoundOutcome.Push);
        }
        else if (PlayerHand.IsBlackjack)
        {
            CompleteRound(RoundOutcome.PlayerBlackjack);
        }
        else
        {
            CompleteRound(RoundOutcome.DealerWin);
        }
    }

    private void CompleteRound(RoundOutcome outcome)
    {
        Outcome = outcome;
        State = RoundState.RoundOver;

        Bankroll += outcome switch
        {
            RoundOutcome.PlayerBlackjack => CurrentBet * 2.5m,
            RoundOutcome.PlayerWin or RoundOutcome.DealerBust => CurrentBet * 2m,
            RoundOutcome.Push => CurrentBet,
            RoundOutcome.Surrendered => CurrentBet * 0.5m,
            _ => 0m
        };
    }

    private void EnsurePlayerTurn()
    {
        if (State != RoundState.PlayerTurn)
        {
            throw new InvalidOperationException("No player turn is active.");
        }
    }
}
