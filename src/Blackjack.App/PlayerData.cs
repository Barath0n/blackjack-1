namespace Blackjack.App;

public sealed class PlayerData
{
    public AppSettings Settings { get; set; } = new();

    public GameStatistics SessionStatistics { get; set; } = new();

    public GameStatistics LifetimeStatistics { get; set; } = new();

    public decimal CurrentBankroll { get; set; } = 500m;

    public DateTimeOffset SessionStartedAt { get; set; } =
        DateTimeOffset.Now;

    public void Normalize()
    {
        Settings ??= new AppSettings();
        SessionStatistics ??= new GameStatistics();
        LifetimeStatistics ??= new GameStatistics();

        Settings.Normalize();

        if (CurrentBankroll < 0)
        {
            CurrentBankroll =
                Settings.StartingBankroll;
        }

        if (SessionStartedAt == default)
        {
            SessionStartedAt =
                DateTimeOffset.Now;
        }

        SessionStatistics.Normalize(
            CurrentBankroll);

        LifetimeStatistics.Normalize(
            CurrentBankroll);
    }
}
