using System.Globalization;
using System.Windows;
using System.Windows.Controls;

namespace Blackjack.App;

public partial class SettingsWindow : Window
{
    private static readonly CultureInfo GermanCulture =
        CultureInfo.GetCultureInfo("de-DE");

    private readonly AppSettings _originalSettings;

    public SettingsWindow(
        AppSettings settings)
    {
        ArgumentNullException.ThrowIfNull(
            settings);

        _originalSettings =
            settings.Clone();

        ResultSettings =
            settings.Clone();

        InitializeComponent();
        PopulateControls();
    }

    public AppSettings ResultSettings { get; private set; }

    private void PopulateControls()
    {
        StartingBankrollInput.Text =
            _originalSettings.StartingBankroll
                .ToString(
                    "0.##",
                    GermanCulture);

        DeckCountSelector.SelectedIndex =
            _originalSettings.DeckCount switch
            {
                1 => 0,
                2 => 1,
                4 => 2,
                6 => 3,
                8 => 4,
                _ => 0
            };

        DealerRuleSelector.SelectedIndex =
            _originalSettings.DealerHitsSoft17
                ? 1
                : 0;
    }

    private void SaveButton_Click(
        object sender,
        RoutedEventArgs e)
    {
        if (!TryParseMoney(
            StartingBankrollInput.Text,
            out decimal startingBankroll) ||
            startingBankroll <= 0)
        {
            MessageBox.Show(
                "Bitte gib ein gültiges Startguthaben größer als 0 € ein.",
                "Ungültiges Startguthaben",
                MessageBoxButton.OK,
                MessageBoxImage.Warning);

            StartingBankrollInput.Focus();
            StartingBankrollInput.SelectAll();
            return;
        }

        if (decimal.Round(
            startingBankroll,
            2,
            MidpointRounding.AwayFromZero) !=
            startingBankroll)
        {
            MessageBox.Show(
                "Das Startguthaben darf höchstens zwei Nachkommastellen haben.",
                "Ungültiges Startguthaben",
                MessageBoxButton.OK,
                MessageBoxImage.Warning);

            return;
        }

        if (startingBankroll > 1_000_000_000m)
        {
            MessageBox.Show(
                "Das Startguthaben darf maximal 1.000.000.000 € betragen.",
                "Ungültiges Startguthaben",
                MessageBoxButton.OK,
                MessageBoxImage.Warning);

            return;
        }

        ComboBoxItem selectedDeck =
            (ComboBoxItem)
                DeckCountSelector.SelectedItem;

        int deckCount = int.Parse(
            selectedDeck.Tag.ToString()!,
            CultureInfo.InvariantCulture);

        ResultSettings = new AppSettings
        {
            StartingBankroll = startingBankroll,
            DeckCount = deckCount,
            DealerHitsSoft17 =
                DealerRuleSelector.SelectedIndex == 1,
            LastBet = _originalSettings.LastBet
        };

        DialogResult = true;
    }

    private void CancelButton_Click(
        object sender,
        RoutedEventArgs e)
    {
        DialogResult = false;
    }

    private static bool TryParseMoney(
        string text,
        out decimal amount)
    {
        string normalized = text
            .Replace("€", string.Empty)
            .Trim();

        return decimal.TryParse(
            normalized,
            NumberStyles.Number,
            GermanCulture,
            out amount) ||
            decimal.TryParse(
                normalized,
                NumberStyles.Number,
                CultureInfo.InvariantCulture,
                out amount);
    }
}
