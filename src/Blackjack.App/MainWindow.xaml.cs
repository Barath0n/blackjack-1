using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Blackjack.Core;

namespace Blackjack.App;

public partial class MainWindow : Window
{
    private static readonly CultureInfo GermanCulture =
        CultureInfo.GetCultureInfo("de-DE");

    private BlackjackGame _game = new();

    public MainWindow()
    {
        InitializeComponent();
        UpdateUi();
    }

    private void DealButton_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            ComboBoxItem selectedItem = (ComboBoxItem)BetSelector.SelectedItem;
            decimal bet = decimal.Parse(
                selectedItem.Tag.ToString()!,
                CultureInfo.InvariantCulture);

            _game.StartRound(bet);
            UpdateUi();
        }
        catch (Exception exception)
        {
            MessageBox.Show(exception.Message, "Blackjack Remastered");
        }
    }

    private void HitButton_Click(object sender, RoutedEventArgs e)
    {
        _game.Hit();
        UpdateUi();
    }

    private void StandButton_Click(object sender, RoutedEventArgs e)
    {
        _game.Stand();
        UpdateUi();
    }

    private void DoubleButton_Click(object sender, RoutedEventArgs e)
    {
        _game.DoubleDown();
        UpdateUi();
    }

    private void SplitButton_Click(object sender, RoutedEventArgs e)
    {
        _game.Split();
        UpdateUi();
    }

    private void SurrenderButton_Click(object sender, RoutedEventArgs e)
    {
        _game.Surrender();
        UpdateUi();
    }

    private void ResetButton_Click(object sender, RoutedEventArgs e)
    {
        _game = new BlackjackGame();
        UpdateUi();
    }

    private void UpdateUi()
    {
        BankrollText.Text = _game.Bankroll.ToString("C", GermanCulture);
        CurrentBetText.Text = _game.CurrentBet.ToString("C", GermanCulture);
        CurrentBetLabel.Text = _game.IsSplitRound
            ? "   Gesamteinsatz "
            : "   Einsatz ";

        DealerCards.ItemsSource = GetDealerCards();

        DealerScoreText.Text = _game.DealerHand.Cards.Count == 0
            ? string.Empty
            : _game.IsDealerHoleCardHidden
                ? $"Sichtbar: {_game.DealerHand.Cards[0].BlackjackValue}"
                : $"Total: {_game.DealerHand.Score}";

        UpdatePlayerHands();

        DealButton.IsEnabled =
            _game.State != RoundState.PlayerTurn &&
            _game.Bankroll > 0;

        HitButton.IsEnabled = _game.CanHit;
        StandButton.IsEnabled = _game.CanStand;
        DoubleButton.IsEnabled = _game.CanDoubleDown;
        SplitButton.IsEnabled = _game.CanSplit;
        SurrenderButton.IsEnabled = _game.CanSurrender;

        StatusText.Text = GetStatusText();
    }

    private void UpdatePlayerHands()
    {
        bool isSplitRound = _game.IsSplitRound;

        Grid.SetColumnSpan(PlayerHand1Border, isSplitRound ? 1 : 2);
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
            cards.ItemsSource = Array.Empty<string>();
            score.Text = string.Empty;
            bet.Text = string.Empty;
            result.Text = string.Empty;
            SetHandBorderState(border, isActive: false);
            return;
        }

        PlayerHandState playerHand = _game.PlayerHands[index];
        bool isActive =
            _game.State == RoundState.PlayerTurn &&
            _game.ActiveHandIndex == index;

        title.Text = _game.IsSplitRound
            ? $"HAND {index + 1}"
            : "SPIELER";

        cards.ItemsSource = playerHand.Hand.Cards
            .Select(card => card.DisplayName)
            .ToArray();

        score.Text = $"Total: {playerHand.Hand.Score}";
        bet.Text = _game.IsSplitRound
            ? $"Einsatz: {playerHand.Bet.ToString("C", GermanCulture)}"
            : string.Empty;
        result.Text = GetHandStatusText(index, playerHand);

        SetHandBorderState(border, isActive);
    }

    private void SetHandBorderState(Border border, bool isActive)
    {
        border.BorderBrush = isActive
            ? (Brush)FindResource("AccentBrush")
            : new SolidColorBrush(Color.FromRgb(55, 107, 83));

        border.BorderThickness = isActive
            ? new Thickness(3)
            : new Thickness(1);
    }

    private string[] GetDealerCards()
    {
        if (_game.DealerHand.Cards.Count == 0)
        {
            return [];
        }

        if (!_game.IsDealerHoleCardHidden)
        {
            return _game.DealerHand.Cards
                .Select(card => card.DisplayName)
                .ToArray();
        }

        return _game.DealerHand.Cards
            .Select((card, index) => index == 1 ? "🂠" : card.DisplayName)
            .ToArray();
    }

    private string GetStatusText() => _game.State switch
    {
        RoundState.WaitingForBet =>
            "Einsatz wählen und die erste Runde starten.",

        RoundState.PlayerTurn when _game.IsSplitRound =>
            $"Hand {_game.ActiveHandIndex + 1} ist am Zug.",

        RoundState.PlayerTurn =>
            "Dein Zug.",

        RoundState.RoundOver when _game.IsSplitRound =>
            string.Join(
                "  ·  ",
                _game.PlayerHands.Select(
                    (hand, index) =>
                        $"Hand {index + 1}: {GetOutcomeText(hand.Outcome)}")),

        RoundState.RoundOver =>
            GetOutcomeText(_game.Outcome),

        _ => string.Empty
    };

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

            if (hand.Outcome == RoundOutcome.PlayerBust)
            {
                return "Über 21";
            }

            if (hand.IsComplete)
            {
                return "Fertig";
            }

            return string.Empty;
        }

        return _game.State == RoundState.RoundOver
            ? GetOutcomeText(hand.Outcome)
            : string.Empty;
    }

    private static string GetOutcomeText(RoundOutcome outcome) => outcome switch
    {
        RoundOutcome.PlayerBlackjack =>
            "Blackjack! Auszahlung 3:2.",

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
            "Aufgegeben – halber Einsatz zurück",

        RoundOutcome.Mixed =>
            "Gemischtes Ergebnis",

        _ =>
            "Runde beendet"
    };
}
