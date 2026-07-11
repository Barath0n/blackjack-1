# Blackjack Remastered

A clean-room modernization of the original 2012 WinForms blackjack project.

## Current status

The remaster currently provides:

- a separate, testable blackjack engine
- a WPF desktop client targeting .NET 10
- correct Ace handling (1 or 11)
- a real 52-card deck without duplicate draws
- hit, stand, double down and late surrender
- split hands with independent stakes and payouts
- double after split and restricted split-Ace handling
- correct blackjack, normal-win, push and surrender payouts
- modern card visuals with suit colors and a custom card back
- animated initial dealing and dealer play
- responsive table layout and round-result feedback
- deterministic regression tests for the adopted house rules

The exact ruleset is documented in [RULES.md](RULES.md).

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

## Planned

- configurable table rules
- multi-deck shoes
- statistics and persistence
- sound effects and additional polish

## Legacy version

The complete original v0.1.9pre source remains preserved on:

- branch: `legacy-v0.1.9pre`
- annotated tag: `v0.1.9pre`

See [MIGRATION.md](MIGRATION.md) for the migration details.
