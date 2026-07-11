# Blackjack Remastered – Ruleset

This document defines the rules currently implemented by the remaster. Blackjack rules vary between casinos, so these choices are treated as explicit house rules rather than universal rules.

## Core rules

- One standard 52-card deck is used.
- The deck is replaced with a freshly shuffled deck when fewer than 15 cards remain before a new round.
- Number cards count at face value.
- Jack, Queen and King count as 10.
- An Ace counts as 11 unless reducing one or more Aces to 1 is necessary to avoid a bust.
- A hand over 21 busts immediately.
- A tie is a push and returns the hand's stake.

## Blackjack

- A natural blackjack is exactly two cards totaling 21 on the original, unsplit hand.
- A winning natural blackjack pays 3:2.
- Player and dealer blackjack is a push.
- The dealer's two-card blackjack is checked immediately after the initial deal (American hole-card / peek behavior).
- A 21 created after a split is not a natural blackjack and pays as a normal 1:1 win.

## Dealer

- The dealer draws on 16 or less.
- The dealer stands on every 17, including soft 17 (S17).
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

## Deliberately not implemented yet

- Insurance
- Even money
- Re-splitting
- Multiple deck shoes
- Configurable H17/S17 behavior
- Configurable double restrictions
- Side bets

These may be added later as optional settings without changing the default ruleset above.
