# Blackjack Remastered – Ruleset

This document defines the default rules and the configurable table options implemented by the remaster. Blackjack rules vary between casinos, so these choices are treated as explicit house rules rather than universal rules.

## Configurable table options

The settings window can start a new session with:

- starting bankroll: any positive amount with up to two decimal places
- shoe size: 1, 2, 4, 6 or 8 standard decks
- dealer behavior on soft 17:
  - **S17**: dealer stands on soft 17 (default)
  - **H17**: dealer hits soft 17

Changing table settings starts a new session and resets the current bankroll to the configured starting bankroll. Lifetime statistics remain intact.

## Core rules

- Each deck contains the standard 52 cards without Jokers.
- The shoe is freshly shuffled before play.
- A new shoe is created before a round when fewer than 15 cards per configured deck remain.
- Number cards count at face value.
- Jack, Queen and King count as 10.
- An Ace counts as 11 unless reducing one or more Aces to 1 is necessary to avoid a bust.
- A hand over 21 busts immediately.
- A tie is a push and returns the hand's stake.
- Monetary payouts are rounded to cents.

## Betting

- The stake may be entered freely.
- The stake must be greater than 0 and may not exceed the current bankroll.
- Up to two decimal places are accepted.
- Quick buttons set the stake to 1%, 5%, 10%, 25% or 100% (`Max`) of the current bankroll.
- The last entered stake is saved locally and restored on the next launch.
- An all-in stake is valid. Double down and split are then unavailable unless enough additional bankroll remains.

## Blackjack

- A natural blackjack is exactly two cards totaling 21 on the original, unsplit hand.
- A winning natural blackjack pays 3:2.
- Player and dealer blackjack is a push.
- The dealer's two-card blackjack is checked immediately after the initial deal (American hole-card / peek behavior).
- A 21 created after a split is not a natural blackjack and pays as a normal 1:1 win.

## Dealer

- The dealer draws on 16 or less.
- With the default **S17** rule, the dealer stands on every 17, including soft 17.
- With **H17** enabled, the dealer draws on soft 17 and stands on hard 17.
- If every player hand has already busted, the dealer does not draw unnecessary cards.

## Double down

- Double down is allowed on any initial two-card player hand.
- The additional stake must be covered by the bankroll.
- Exactly one additional card is drawn and the hand then stands automatically.
- Double after split (DAS) is allowed.
- Split Aces are resolved automatically and therefore cannot be doubled.

## Split

- Two initial cards may be split when they have the same blackjack value.
- This means mixed ten-value cards such as 10 + Queen or Jack + King may be split.
- A second stake equal to the original hand stake is required.
- The current version allows one split only, producing a maximum of two player hands.
- Re-splitting is not currently available.
- Both split hands are settled independently against the same dealer hand.
- Split Aces receive exactly one additional card each and then stand automatically.
- A two-card 21 after a split is treated as a normal 21, not a natural blackjack.

## Surrender

- Late surrender is available only on the original two-card hand.
- Surrender is unavailable after hitting, doubling or splitting.
- The dealer blackjack check happens before surrender can be offered.
- Surrender returns half of the hand's stake.

## Persistence and statistics

The app stores its player data locally in:

```text
%LOCALAPPDATA%\BlackjackRemastered\player-data.json
```

The saved data contains:

- table settings
- last entered stake
- current bankroll
- current session start time
- current session statistics
- lifetime statistics

A completed round updates both the session and lifetime statistics. Closing the app during an unfinished round does not permanently deduct that unfinished round's stake; the last completed bankroll is restored on the next launch.

## Deliberately not implemented yet

- Insurance
- Even money
- Re-splitting
- configurable double restrictions
- side bets

These may be added later without changing the default ruleset above.
