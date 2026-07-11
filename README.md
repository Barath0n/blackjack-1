# Blackjack Remastered

A clean-room modernization of the original 2012 WinForms blackjack project.

## Current status

The first remaster milestone provides:

- a separate, testable blackjack engine
- a WPF desktop client targeting .NET 10
- correct ace handling (1 or 11)
- a real 52-card deck without duplicate draws
- hit, stand, double down and surrender
- correct normal-win, blackjack, push and surrender payouts
- a responsive starter UI without external assets

Split hands, persistence, statistics, card artwork, animations and sound are planned for later milestones.

## Requirements

- Windows 10/11
- Visual Studio 2022 or newer with the **.NET desktop development** workload
- .NET 10 SDK

## Run

```powershell
dotnet restore
dotnet build
dotnet run --project .\src\Blackjack.App\Blackjack.App.csproj
```

## Tests

```powershell
dotnet test
```

## Legacy version

The complete original v0.1.9pre source should remain preserved on:

- branch: `legacy-v0.1.9pre`
- annotated tag: `v0.1.9pre`

See [MIGRATION.md](MIGRATION.md) for the exact commands.
