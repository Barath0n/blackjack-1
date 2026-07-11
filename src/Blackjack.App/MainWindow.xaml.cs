using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using Blackjack.Core;

namespace Blackjack.App;

public partial class MainWindow : Window
{
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
        BankrollText.Text = _game.Bankroll.ToString("C", CultureInfo.GetCultureInfo("de-DE"));
        CurrentBetText.Text = _game.CurrentBet.ToString("C", CultureInfo.GetCultureInfo("de-DE"));

        PlayerCards.ItemsSource = _game.PlayerHand.Cards
            .Select(card => card.DisplayName)
            .ToArray();

        DealerCards.ItemsSource = GetDealerCards();

        PlayerScoreText.Text = _game.PlayerHand.Cards.Count == 0
            ? string.Empty
            : $"Total: {_game.PlayerHand.Score}";

        DealerScoreText.Text = _game.DealerHand.Cards.Count == 0
    		? string.Empty
    		: _game.IsDealerHoleCardHidden
        		? $"Sichtbar: {_game.DealerHand.Cards[0].BlackjackValue}"
        		: $"Total: {_game.DealerHand.Score}";

        DealButton.IsEnabled =
            _game.State != RoundState.PlayerTurn &&
            _game.Bankroll > 0;

        HitButton.IsEnabled = _game.CanHit;
        StandButton.IsEnabled = _game.CanStand;
        DoubleButton.IsEnabled = _game.CanDoubleDown;
        SurrenderButton.IsEnabled = _game.CanSurrender;

        StatusText.Text = GetStatusText();
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
        RoundState.WaitingForBet => "Einsatz wählen und die erste Runde starten.",
        RoundState.PlayerTurn => "Dein Zug.",
        RoundState.RoundOver => _game.Outcome switch
        {
            RoundOutcome.PlayerBlackjack => "Blackjack! Auszahlung 3:2.",
            RoundOutcome.PlayerWin => "Du gewinnst.",
            RoundOutcome.DealerWin => "Der Dealer gewinnt.",
            RoundOutcome.Push => "Unentschieden – dein Einsatz wird zurückgegeben.",
            RoundOutcome.PlayerBust => "Über 21 – du hast verloren.",
            RoundOutcome.DealerBust => "Der Dealer ist über 21 – du gewinnst.",
            RoundOutcome.Surrendered => "Aufgegeben – die Hälfte des Einsatzes wurde zurückgegeben.",
            _ => "Runde beendet."
        },
        _ => string.Empty
    };
}
