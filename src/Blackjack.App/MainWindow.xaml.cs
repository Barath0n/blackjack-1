using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using Blackjack.Core;

namespace Blackjack.App;

public partial class MainWindow : Window
{
    private const int InitialPauseMilliseconds = 140;
    private const int CardDealDelayMilliseconds = 230;
    private const int DealerRevealDelayMilliseconds = 430;
    private const int DealerThinkDelayMilliseconds = 380;

    private static readonly CultureInfo GermanCulture =
        CultureInfo.GetCultureInfo("de-DE");

    private readonly int[] _visiblePlayerCardCounts =
        [int.MaxValue, int.MaxValue];

    private BlackjackGame _game = new();
    private bool _isBusy;
    private bool _forceDealerHoleCardHidden;
    private bool _suppressRoundResult;
    private int _visibleDealerCardCount = int.MaxValue;
    private string? _statusOverride;
    private decimal? _displayBankrollOverride;
    private decimal _roundStartingBankroll = 500m;

    public MainWindow()
    {
        InitializeComponent();
        UpdateUi();
    }

    private async void DealButton_Click(
        object sender,
        RoutedEventArgs e)
    {
        await RunAnimatedActionAsync(async () =>
        {
            ComboBoxItem selectedItem =
                (ComboBoxItem)BetSelector.SelectedItem;

            decimal bet = decimal.Parse(
                selectedItem.Tag.ToString()!,
                CultureInfo.InvariantCulture);

            await AnimateInitialDealAsync(bet);
        });
    }

    private async void HitButton_Click(
        object sender,
        RoutedEventArgs e)
    {
        await RunAnimatedActionAsync(AnimateHitAsync);
    }

    private async void StandButton_Click(
        object sender,
        RoutedEventArgs e)
    {
        await RunAnimatedActionAsync(AnimateStandAsync);
    }

    private async void DoubleButton_Click(
        object sender,
        RoutedEventArgs e)
    {
        await RunAnimatedActionAsync(AnimateDoubleDownAsync);
    }

    private async void SplitButton_Click(
        object sender,
        RoutedEventArgs e)
    {
        await RunAnimatedActionAsync(AnimateSplitAsync);
    }

    private async void SurrenderButton_Click(
        object sender,
        RoutedEventArgs e)
    {
        await RunAnimatedActionAsync(AnimateSurrenderAsync);
    }

    private void ResetButton_Click(
        object sender,
        RoutedEventArgs e)
    {
        if (_isBusy)
        {
            return;
        }

        _game = new BlackjackGame();
        _roundStartingBankroll = _game.Bankroll;
        ResetAnimationOverrides();
        UpdateUi();
    }

    private async Task RunAnimatedActionAsync(
        Func<Task> action)
    {
        if (_isBusy)
        {
            return;
        }

        _isBusy = true;
        UpdateUi();

        try
        {
            await action();
        }
        catch (Exception exception)
        {
            MessageBox.Show(
                exception.Message,
                "Blackjack Remastered");
        }
        finally
        {
            _isBusy = false;
            ResetAnimationOverrides();
            UpdateUi();
        }
    }

    private async Task AnimateInitialDealAsync(
        decimal bet)
    {
        _roundStartingBankroll = _game.Bankroll;
        decimal bankrollAfterBet = _game.Bankroll - bet;

        _game.StartRound(bet);

        _displayBankrollOverride = bankrollAfterBet;
        _suppressRoundResult = true;
        _forceDealerHoleCardHidden = true;
        _visibleDealerCardCount = 0;
        _visiblePlayerCardCounts[0] = 0;
        _visiblePlayerCardCounts[1] = 0;
        _statusOverride = "Karten werden ausgeteilt …";

        UpdateUi();
        await Task.Delay(InitialPauseMilliseconds);

        await RevealPlayerCardAsync(
            handIndex: 0,
            visibleCount: 1,
            message: "Du erhältst die erste Karte …");

        await RevealDealerCardAsync(
            visibleCount: 1,
            message: "Der Dealer erhält eine Karte …");

        await RevealPlayerCardAsync(
            handIndex: 0,
            visibleCount: 2,
            message: "Du erhältst die zweite Karte …");

        await RevealDealerCardAsync(
            visibleCount: 2,
            message: "Der Dealer erhält eine verdeckte Karte …");

        if (_game.State == RoundState.RoundOver)
        {
            await AnimateDealerResolutionAsync(
                dealerCardsBefore: 2,
                bankrollWhileDealerActs: bankrollAfterBet,
                openingMessage: "Blackjack wird geprüft …");

            return;
        }

        ReturnToCurrentGameState();
    }

    private async Task AnimateHitAsync()
    {
        int handIndex = _game.ActiveHandIndex;
        int playerCardsBefore =
            _game.PlayerHands[handIndex].Hand.Cards.Count;

        int dealerCardsBefore = _game.DealerHand.Cards.Count;
        decimal bankrollBefore = _game.Bankroll;

        _suppressRoundResult = true;
        _forceDealerHoleCardHidden = true;
        _visibleDealerCardCount = dealerCardsBefore;
        _visiblePlayerCardCounts[handIndex] = playerCardsBefore;
        _displayBankrollOverride = bankrollBefore;

        _game.Hit();

        _statusOverride = "Karte wird ausgeteilt …";
        UpdateUi();

        await Task.Delay(InitialPauseMilliseconds);

        _visiblePlayerCardCounts[handIndex] =
            _game.PlayerHands[handIndex].Hand.Cards.Count;

        UpdateUi();
        await Task.Delay(CardDealDelayMilliseconds);

        if (_game.State == RoundState.RoundOver)
        {
            await AnimateDealerResolutionAsync(
                dealerCardsBefore,
                bankrollBefore);

            return;
        }

        ReturnToCurrentGameState();
    }

    private async Task AnimateStandAsync()
    {
        int dealerCardsBefore = _game.DealerHand.Cards.Count;
        decimal bankrollBefore = _game.Bankroll;

        _suppressRoundResult = true;
        _forceDealerHoleCardHidden = true;
        _visibleDealerCardCount = dealerCardsBefore;
        _displayBankrollOverride = bankrollBefore;
        _statusOverride = _game.IsSplitRound
            ? $"Hand {_game.ActiveHandIndex + 1} hält."
            : "Du hältst.";

        UpdateUi();
        await Task.Delay(CardDealDelayMilliseconds);

        _game.Stand();

        if (_game.State == RoundState.RoundOver)
        {
            await AnimateDealerResolutionAsync(
                dealerCardsBefore,
                bankrollBefore);

            return;
        }

        ReturnToCurrentGameState();
    }

    private async Task AnimateDoubleDownAsync()
    {
        int handIndex = _game.ActiveHandIndex;
        int playerCardsBefore =
            _game.PlayerHands[handIndex].Hand.Cards.Count;

        int dealerCardsBefore = _game.DealerHand.Cards.Count;
        decimal bankrollBefore = _game.Bankroll;
        decimal additionalBet = _game.ActiveHand!.Bet;
        decimal bankrollAfterDouble =
            bankrollBefore - additionalBet;

        _suppressRoundResult = true;
        _forceDealerHoleCardHidden = true;
        _visibleDealerCardCount = dealerCardsBefore;
        _visiblePlayerCardCounts[handIndex] = playerCardsBefore;
        _displayBankrollOverride = bankrollAfterDouble;

        _game.DoubleDown();

        _statusOverride = "Einsatz verdoppelt – eine letzte Karte …";
        UpdateUi();

        await Task.Delay(InitialPauseMilliseconds);

        _visiblePlayerCardCounts[handIndex] =
            _game.PlayerHands[handIndex].Hand.Cards.Count;

        UpdateUi();
        await Task.Delay(CardDealDelayMilliseconds);

        if (_game.State == RoundState.RoundOver)
        {
            await AnimateDealerResolutionAsync(
                dealerCardsBefore,
                bankrollAfterDouble);

            return;
        }

        ReturnToCurrentGameState();
    }

    private async Task AnimateSplitAsync()
    {
        int dealerCardsBefore = _game.DealerHand.Cards.Count;
        decimal bankrollBefore = _game.Bankroll;
        decimal splitBet = _game.ActiveHand!.Bet;
        decimal bankrollAfterSplit =
            bankrollBefore - splitBet;

        _suppressRoundResult = true;
        _forceDealerHoleCardHidden = true;
        _visibleDealerCardCount = dealerCardsBefore;
        _displayBankrollOverride = bankrollAfterSplit;

        _game.Split();

        _visiblePlayerCardCounts[0] = 1;
        _visiblePlayerCardCounts[1] = 1;
        _statusOverride = "Die Hand wird geteilt …";

        UpdateUi();
        await Task.Delay(CardDealDelayMilliseconds);

        _visiblePlayerCardCounts[0] = 2;
        _statusOverride = "Hand 1 erhält eine Karte …";

        UpdateUi();
        await Task.Delay(CardDealDelayMilliseconds);

        _visiblePlayerCardCounts[1] = 2;
        _statusOverride = "Hand 2 erhält eine Karte …";

        UpdateUi();
        await Task.Delay(CardDealDelayMilliseconds);

        if (_game.State == RoundState.RoundOver)
        {
            await AnimateDealerResolutionAsync(
                dealerCardsBefore,
                bankrollAfterSplit);

            return;
        }

        ReturnToCurrentGameState();
    }

    private async Task AnimateSurrenderAsync()
    {
        int dealerCardsBefore = _game.DealerHand.Cards.Count;
        decimal bankrollBefore = _game.Bankroll;

        _suppressRoundResult = true;
        _forceDealerHoleCardHidden = true;
        _visibleDealerCardCount = dealerCardsBefore;
        _displayBankrollOverride = bankrollBefore;

        _game.Surrender();

        await AnimateDealerResolutionAsync(
            dealerCardsBefore,
            bankrollBefore,
            openingMessage: "Du gibst auf …");
    }

    private async Task RevealPlayerCardAsync(
        int handIndex,
        int visibleCount,
        string message)
    {
        _visiblePlayerCardCounts[handIndex] = visibleCount;
        _statusOverride = message;

        UpdateUi();
        await Task.Delay(CardDealDelayMilliseconds);
    }

    private async Task RevealDealerCardAsync(
        int visibleCount,
        string message)
    {
        _visibleDealerCardCount = visibleCount;
        _statusOverride = message;

        UpdateUi();
        await Task.Delay(CardDealDelayMilliseconds);
    }

    private async Task AnimateDealerResolutionAsync(
        int dealerCardsBefore,
        decimal bankrollWhileDealerActs,
        string openingMessage = "Der Dealer ist am Zug …")
    {
        int finalDealerCardCount =
            _game.DealerHand.Cards.Count;

        _suppressRoundResult = true;
        _displayBankrollOverride = bankrollWhileDealerActs;
        _visibleDealerCardCount = Math.Min(
            dealerCardsBefore,
            finalDealerCardCount);

        _forceDealerHoleCardHidden = true;
        _statusOverride = openingMessage;

        UpdateUi();
        await Task.Delay(DealerRevealDelayMilliseconds);

        _forceDealerHoleCardHidden = false;
        _statusOverride = "Der Dealer deckt auf …";

        UpdateUi();
        await Task.Delay(DealerRevealDelayMilliseconds);

        for (
            int visibleCount = dealerCardsBefore + 1;
            visibleCount <= finalDealerCardCount;
            visibleCount++)
        {
            _statusOverride = "Der Dealer zieht eine Karte …";
            UpdateUi();

            await Task.Delay(DealerThinkDelayMilliseconds);

            _visibleDealerCardCount = visibleCount;
            UpdateUi();

            await Task.Delay(CardDealDelayMilliseconds);
        }

        _displayBankrollOverride = null;
        _suppressRoundResult = false;
        _statusOverride = null;
        _visibleDealerCardCount = int.MaxValue;

        UpdateUi();
        await AnimateResultBannerAsync();
    }

    private async Task AnimateResultBannerAsync()
    {
        QuadraticEase easing = new()
        {
            EasingMode = EasingMode.EaseOut
        };

        DoubleAnimation opacityAnimation = new(
            fromValue: 0.55,
            toValue: 1,
            duration: TimeSpan.FromMilliseconds(260))
        {
            EasingFunction = easing,
            FillBehavior = FillBehavior.Stop
        };

        DoubleAnimation scaleAnimation = new(
            fromValue: 0.96,
            toValue: 1,
            duration: TimeSpan.FromMilliseconds(260))
        {
            EasingFunction = easing,
            FillBehavior = FillBehavior.Stop
        };

        StatusBorder.BeginAnimation(
            UIElement.OpacityProperty,
            opacityAnimation);

        StatusScale.BeginAnimation(
            ScaleTransform.ScaleXProperty,
            scaleAnimation);

        StatusScale.BeginAnimation(
            ScaleTransform.ScaleYProperty,
            scaleAnimation);

        await Task.Delay(280);
    }

    private void ReturnToCurrentGameState()
    {
        _displayBankrollOverride = null;
        _suppressRoundResult = false;
        _forceDealerHoleCardHidden = false;
        _statusOverride = null;

        ShowAllCards();
        UpdateUi();
    }

    private void ResetAnimationOverrides()
    {
        _displayBankrollOverride = null;
        _suppressRoundResult = false;
        _forceDealerHoleCardHidden = false;
        _statusOverride = null;

        ShowAllCards();
    }

    private void ShowAllCards()
    {
        _visibleDealerCardCount = int.MaxValue;
        Array.Fill(
            _visiblePlayerCardCounts,
            int.MaxValue);
    }

    private void UpdateUi()
    {
        decimal displayedBankroll =
            _displayBankrollOverride ?? _game.Bankroll;

        BankrollText.Text = displayedBankroll.ToString(
            "C",
            GermanCulture);

        CurrentBetText.Text = _game.CurrentBet.ToString(
            "C",
            GermanCulture);

        CurrentBetLabel.Text = _game.IsSplitRound
            ? "   Gesamteinsatz "
            : "   Einsatz ";

        DealerCards.ItemsSource = GetDealerCards();
        UpdateDealerScore();
        UpdatePlayerHands();

        bool interactionEnabled = !_isBusy;

        DealButton.IsEnabled =
            interactionEnabled &&
            _game.State != RoundState.PlayerTurn &&
            _game.Bankroll > 0;

        BetSelector.IsEnabled =
            interactionEnabled &&
            _game.State != RoundState.PlayerTurn;

        HitButton.IsEnabled =
            interactionEnabled &&
            _game.CanHit;

        StandButton.IsEnabled =
            interactionEnabled &&
            _game.CanStand;

        DoubleButton.IsEnabled =
            interactionEnabled &&
            _game.CanDoubleDown;

        SplitButton.IsEnabled =
            interactionEnabled &&
            _game.CanSplit;

        SurrenderButton.IsEnabled =
            interactionEnabled &&
            _game.CanSurrender;

        ResetButton.IsEnabled = interactionEnabled;

        StatusText.Text = GetStatusText();
        ApplyStatusAppearance();
    }

    private void UpdateDealerScore()
    {
        int visibleCount = Math.Min(
            _visibleDealerCardCount,
            _game.DealerHand.Cards.Count);

        if (visibleCount == 0)
        {
            DealerScoreText.Text = string.Empty;
            return;
        }

        bool hideHoleCard = ShouldHideDealerHoleCard();

        Card[] visibleCards = _game.DealerHand.Cards
            .Take(visibleCount)
            .Where(
                (_, index) =>
                    !(hideHoleCard && index == 1))
            .ToArray();

        if (visibleCards.Length == 0)
        {
            DealerScoreText.Text = string.Empty;
            return;
        }

        int score = CalculateScore(visibleCards);

        DealerScoreText.Text = hideHoleCard
            ? $"Sichtbar: {score}"
            : $"Total: {score}";
    }

    private void UpdatePlayerHands()
    {
        bool isSplitRound = _game.IsSplitRound;

        Grid.SetColumnSpan(
            PlayerHand1Border,
            isSplitRound ? 1 : 2);

        PlayerHand2Border.Visibility = isSplitRound
            ? Visibility.Visible
            : Visibility.Collapsed;

        UpdatePlayerHand(
            index: 0,
            border: PlayerHand1Border,
            title: PlayerHand1Title,
            cards: PlayerCards1,
            score: PlayerScore1Text,
            bet: PlayerBet1Text,
            result: PlayerResult1Text);

        if (isSplitRound)
        {
            UpdatePlayerHand(
                index: 1,
                border: PlayerHand2Border,
                title: PlayerHand2Title,
                cards: PlayerCards2,
                score: PlayerScore2Text,
                bet: PlayerBet2Text,
                result: PlayerResult2Text);
        }
    }

    private void UpdatePlayerHand(
        int index,
        Border border,
        TextBlock title,
        ItemsControl cards,
        TextBlock score,
        TextBlock bet,
        TextBlock result)
    {
        if (index >= _game.PlayerHands.Count)
        {
            title.Text = "SPIELER";
            cards.ItemsSource =
                Array.Empty<CardVisualModel>();

            score.Text = string.Empty;
            bet.Text = string.Empty;
            result.Text = string.Empty;

            SetHandBorderState(
                border,
                isActive: false);

            return;
        }

        PlayerHandState playerHand =
            _game.PlayerHands[index];

        bool isActive =
            _game.State == RoundState.PlayerTurn &&
            _game.ActiveHandIndex == index;

        title.Text = _game.IsSplitRound
            ? $"HAND {index + 1}"
            : "SPIELER";

        int visibleCount = Math.Min(
            _visiblePlayerCardCounts[index],
            playerHand.Hand.Cards.Count);

        Card[] visibleCards = playerHand.Hand.Cards
            .Take(visibleCount)
            .ToArray();

        cards.ItemsSource = visibleCards
            .Select(CardVisualModel.FromCard)
            .ToArray();

        score.Text = visibleCards.Length == 0
            ? string.Empty
            : $"Total: {CalculateScore(visibleCards)}";

        bet.Text = _game.IsSplitRound
            ? $"Einsatz: {playerHand.Bet.ToString(
                "C",
                GermanCulture)}"
            : string.Empty;

        result.Text = GetHandStatusText(
            index,
            playerHand);

        SetHandBorderState(
            border,
            isActive);
    }

    private void SetHandBorderState(
        Border border,
        bool isActive)
    {
        border.BorderBrush = isActive
            ? (Brush)FindResource("AccentBrush")
            : new SolidColorBrush(
                Color.FromRgb(55, 107, 83));

        border.BorderThickness = isActive
            ? new Thickness(3)
            : new Thickness(1);
    }

    private CardVisualModel[] GetDealerCards()
    {
        int visibleCount = Math.Min(
            _visibleDealerCardCount,
            _game.DealerHand.Cards.Count);

        if (visibleCount == 0)
        {
            return [];
        }

        bool hideHoleCard = ShouldHideDealerHoleCard();

        return _game.DealerHand.Cards
            .Take(visibleCount)
            .Select(
                (card, index) =>
                    hideHoleCard && index == 1
                        ? CardVisualModel.FaceDown
                        : CardVisualModel.FromCard(card))
            .ToArray();
    }

    private bool ShouldHideDealerHoleCard() =>
        _forceDealerHoleCardHidden ||
        _game.IsDealerHoleCardHidden;

    private string GetStatusText()
    {
        if (!string.IsNullOrWhiteSpace(
            _statusOverride))
        {
            return _statusOverride;
        }

        return _game.State switch
        {
            RoundState.WaitingForBet =>
                "Einsatz wählen und die erste Runde starten.",

            RoundState.PlayerTurn
                when _game.IsSplitRound =>
                $"Hand {_game.ActiveHandIndex + 1} ist am Zug.",

            RoundState.PlayerTurn =>
                "Dein Zug.",

            RoundState.RoundOver
                when _game.IsSplitRound =>
                GetSplitRoundResultText(),

            RoundState.RoundOver =>
                $"{GetOutcomeText(_game.Outcome)}" +
                $"{GetRoundBalanceText()}",

            _ =>
                string.Empty
        };
    }

    private string GetSplitRoundResultText()
    {
        string handResults = string.Join(
            "  ·  ",
            _game.PlayerHands.Select(
                (hand, index) =>
                    $"Hand {index + 1}: " +
                    $"{GetCompactOutcomeText(
                        hand.Outcome)}"));

        return handResults + GetRoundBalanceText();
    }

    private string GetRoundBalanceText()
    {
        decimal difference =
            _game.Bankroll - _roundStartingBankroll;

        if (difference > 0)
        {
            return "  •  Gewinn: +" +
                difference.ToString(
                    "C",
                    GermanCulture);
        }

        if (difference < 0)
        {
            return "  •  Verlust: -" +
                Math.Abs(difference).ToString(
                    "C",
                    GermanCulture);
        }

        return "  •  Bilanz: ±0,00 €";
    }

    private string GetHandStatusText(
        int index,
        PlayerHandState hand)
    {
        if (_game.State == RoundState.PlayerTurn)
        {
            if (_game.ActiveHandIndex == index)
            {
                return "Am Zug";
            }

            if (hand.Outcome ==
                RoundOutcome.PlayerBust)
            {
                return "Über 21";
            }

            if (hand.IsComplete)
            {
                return "Fertig";
            }

            return string.Empty;
        }

        if (
            _suppressRoundResult &&
            _game.State == RoundState.RoundOver)
        {
            return hand.Outcome ==
                RoundOutcome.PlayerBust
                    ? "Über 21"
                    : string.Empty;
        }

        return _game.State == RoundState.RoundOver
            ? GetCompactOutcomeText(hand.Outcome)
            : string.Empty;
    }

    private void ApplyStatusAppearance()
    {
        bool showResult =
            _game.State == RoundState.RoundOver &&
            !_suppressRoundResult &&
            string.IsNullOrWhiteSpace(
                _statusOverride);

        if (!showResult)
        {
            StatusBorder.Background =
                new SolidColorBrush(
                    Color.FromArgb(
                        170,
                        7,
                        26,
                        18));

            StatusBorder.BorderBrush =
                Brushes.Transparent;

            StatusBorder.BorderThickness =
                new Thickness(0);

            StatusText.Foreground =
                (Brush)FindResource(
                    "AccentBrush");

            StatusText.FontSize = 18;
            return;
        }

        ResultTone tone = GetResultTone();

        (Color background, Color border) =
            tone switch
            {
                ResultTone.Positive =>
                    (
                        Color.FromArgb(
                            225,
                            21,
                            86,
                            55),
                        Color.FromRgb(
                            84,
                            190,
                            123)
                    ),

                ResultTone.Negative =>
                    (
                        Color.FromArgb(
                            225,
                            92,
                            31,
                            38),
                        Color.FromRgb(
                            220,
                            91,
                            100)
                    ),

                ResultTone.Mixed =>
                    (
                        Color.FromArgb(
                            225,
                            91,
                            70,
                            25),
                        Color.FromRgb(
                            231,
                            183,
                            91)
                    ),

                _ =>
                    (
                        Color.FromArgb(
                            225,
                            36,
                            57,
                            73),
                        Color.FromRgb(
                            104,
                            157,
                            194)
                    )
            };

        StatusBorder.Background =
            new SolidColorBrush(background);

        StatusBorder.BorderBrush =
            new SolidColorBrush(border);

        StatusBorder.BorderThickness =
            new Thickness(2);

        StatusText.Foreground = Brushes.White;
        StatusText.FontSize = 20;
    }

    private ResultTone GetResultTone()
    {
        RoundOutcome[] outcomes =
            _game.PlayerHands
                .Select(hand => hand.Outcome)
                .ToArray();

        bool hasWin = outcomes.Any(
            outcome => outcome is
                RoundOutcome.PlayerBlackjack or
                RoundOutcome.PlayerWin or
                RoundOutcome.DealerBust);

        bool hasLoss = outcomes.Any(
            outcome => outcome is
                RoundOutcome.DealerWin or
                RoundOutcome.PlayerBust or
                RoundOutcome.Surrendered);

        if (hasWin && hasLoss)
        {
            return ResultTone.Mixed;
        }

        if (hasWin)
        {
            return ResultTone.Positive;
        }

        if (hasLoss)
        {
            return ResultTone.Negative;
        }

        return ResultTone.Neutral;
    }

    private static int CalculateScore(
        IEnumerable<Card> cards)
    {
        Hand hand = new();

        foreach (Card card in cards)
        {
            hand.Add(card);
        }

        return hand.Score;
    }

    private static string GetOutcomeText(
        RoundOutcome outcome) =>
        outcome switch
        {
            RoundOutcome.PlayerBlackjack =>
                "Blackjack! Auszahlung 3:2.",

            RoundOutcome.PlayerWin =>
                "Du gewinnst!",

            RoundOutcome.DealerWin =>
                "Der Dealer gewinnt.",

            RoundOutcome.Push =>
                "Unentschieden – Einsatz zurück.",

            RoundOutcome.PlayerBust =>
                "Über 21 – du hast verloren.",

            RoundOutcome.DealerBust =>
                "Der Dealer ist über 21 – du gewinnst!",

            RoundOutcome.Surrendered =>
                "Aufgegeben – halber Einsatz zurück.",

            RoundOutcome.Mixed =>
                "Gemischtes Ergebnis.",

            _ =>
                "Runde beendet."
        };

    private static string GetCompactOutcomeText(
        RoundOutcome outcome) =>
        outcome switch
        {
            RoundOutcome.PlayerBlackjack =>
                "Blackjack",

            RoundOutcome.PlayerWin =>
                "Gewonnen",

            RoundOutcome.DealerWin =>
                "Verloren",

            RoundOutcome.Push =>
                "Unentschieden",

            RoundOutcome.PlayerBust =>
                "Verloren – über 21",

            RoundOutcome.DealerBust =>
                "Gewonnen – Dealer über 21",

            RoundOutcome.Surrendered =>
                "Aufgegeben",

            _ =>
                "Beendet"
        };

    private enum ResultTone
    {
        Neutral,
        Positive,
        Negative,
        Mixed
    }
}
