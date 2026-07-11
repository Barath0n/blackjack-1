using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Blackjack.App;

public partial class StatisticsWindow : Window
{
    private static readonly CultureInfo GermanCulture =
        CultureInfo.GetCultureInfo("de-DE");

    private readonly GameStatistics _sessionStatistics;
    private readonly GameStatistics _lifetimeStatistics;
    private readonly decimal _currentBankroll;

    public StatisticsWindow(
        GameStatistics sessionStatistics,
        GameStatistics lifetimeStatistics,
        decimal currentBankroll,
        DateTimeOffset sessionStartedAt)
    {
        ArgumentNullException.ThrowIfNull(
            sessionStatistics);

        ArgumentNullException.ThrowIfNull(
            lifetimeStatistics);

        _sessionStatistics =
            sessionStatistics;

        _lifetimeStatistics =
            lifetimeStatistics;

        _currentBankroll =
            currentBankroll;

        SessionStartedAt =
            sessionStartedAt;

        InitializeComponent();
        UpdateUi();
    }

    public bool DataChanged { get; private set; }

    public DateTimeOffset SessionStartedAt { get; private set; }

    private void UpdateUi()
    {
        SessionStartedText.Text =
            $"Session seit {SessionStartedAt.ToLocalTime():dd.MM.yyyy, HH:mm} Uhr";

        CurrentBankrollText.Text =
            $"Aktuelles Guthaben: {_currentBankroll.ToString("C", GermanCulture)}";

        UpdateColumn(
            _sessionStatistics,
            SessionRoundsText,
            SessionHandsText,
            SessionWinsText,
            SessionLossesText,
            SessionPushesText,
            SessionBlackjacksText,
            SessionSurrendersText,
            SessionWinRateText,
            SessionWageredText,
            SessionProfitText,
            SessionBiggestWinText,
            SessionBiggestLossText,
            SessionHighestBankrollText);

        UpdateColumn(
            _lifetimeStatistics,
            LifetimeRoundsText,
            LifetimeHandsText,
            LifetimeWinsText,
            LifetimeLossesText,
            LifetimePushesText,
            LifetimeBlackjacksText,
            LifetimeSurrendersText,
            LifetimeWinRateText,
            LifetimeWageredText,
            LifetimeProfitText,
            LifetimeBiggestWinText,
            LifetimeBiggestLossText,
            LifetimeHighestBankrollText);
    }

    private static void UpdateColumn(
        GameStatistics statistics,
        TextBlock rounds,
        TextBlock hands,
        TextBlock wins,
        TextBlock losses,
        TextBlock pushes,
        TextBlock blackjacks,
        TextBlock surrenders,
        TextBlock winRate,
        TextBlock wagered,
        TextBlock profit,
        TextBlock biggestWin,
        TextBlock biggestLoss,
        TextBlock highestBankroll)
    {
        rounds.Text =
            statistics.RoundsPlayed.ToString(
                "N0",
                GermanCulture);

        hands.Text =
            statistics.HandsPlayed.ToString(
                "N0",
                GermanCulture);

        wins.Text =
            statistics.HandsWon.ToString(
                "N0",
                GermanCulture);

        losses.Text =
            statistics.HandsLost.ToString(
                "N0",
                GermanCulture);

        pushes.Text =
            statistics.HandsPushed.ToString(
                "N0",
                GermanCulture);

        blackjacks.Text =
            statistics.Blackjacks.ToString(
                "N0",
                GermanCulture);

        surrenders.Text =
            statistics.Surrenders.ToString(
                "N0",
                GermanCulture);

        winRate.Text =
            statistics.WinRate.ToString(
                "P1",
                GermanCulture);

        wagered.Text =
            statistics.TotalWagered.ToString(
                "C",
                GermanCulture);

        profit.Text =
            FormatSignedMoney(
                statistics.NetProfit);

        biggestWin.Text =
            statistics.BiggestRoundWin.ToString(
                "C",
                GermanCulture);

        biggestLoss.Text =
            statistics.BiggestRoundLoss.ToString(
                "C",
                GermanCulture);

        highestBankroll.Text =
            statistics.HighestBankroll.ToString(
                "C",
                GermanCulture);

        profit.Foreground =
            statistics.NetProfit switch
            {
                > 0m => new SolidColorBrush(
                    Color.FromRgb(
                        91,
                        210,
                        132)),

                < 0m => new SolidColorBrush(
                    Color.FromRgb(
                        235,
                        107,
                        115)),

                _ => Brushes.White
            };
    }

    private void ResetSessionStatisticsButton_Click(
        object sender,
        RoutedEventArgs e)
    {
        MessageBoxResult result =
            MessageBox.Show(
                "Möchtest du nur die gespeicherte Statistik der aktuellen Session löschen? Das aktuelle Guthaben bleibt erhalten.",
                "Session-Statistik löschen",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

        if (result != MessageBoxResult.Yes)
        {
            return;
        }

        _sessionStatistics.Reset(
            _currentBankroll);

        SessionStartedAt =
            DateTimeOffset.Now;

        DataChanged = true;
        UpdateUi();
    }

    private void ResetLifetimeStatisticsButton_Click(
        object sender,
        RoutedEventArgs e)
    {
        MessageBoxResult result =
            MessageBox.Show(
                "Möchtest du die gesamte gespeicherte Langzeitstatistik wirklich löschen? Diese Aktion kann nicht rückgängig gemacht werden.",
                "Gesamtstatistik löschen",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

        if (result != MessageBoxResult.Yes)
        {
            return;
        }

        _lifetimeStatistics.Reset(
            _currentBankroll);

        DataChanged = true;
        UpdateUi();
    }

    private void CloseButton_Click(
        object sender,
        RoutedEventArgs e)
    {
        Close();
    }

    private static string FormatSignedMoney(
        decimal value)
    {
        if (value > 0m)
        {
            return "+" +
                value.ToString(
                    "C",
                    GermanCulture);
        }

        if (value < 0m)
        {
            return "-" +
                Math.Abs(value).ToString(
                    "C",
                    GermanCulture);
        }

        return 0m.ToString(
            "C",
            GermanCulture);
    }
}
