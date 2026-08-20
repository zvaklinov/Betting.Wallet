## Overview
 
The application starts a player at a $0 balance and accepts four commands from standard input:
 
```text
deposit <amount>
withdraw <amount>
bet <amount>
exit
```
 
Every successful operation prints the resulting balance. Bets must be between $1 and $10 and are resolved against the following distribution:
 
```text
50% -> lose
40% -> win up to 2x the bet
10% -> win between 2x and 10x the bet
```
 
The new balance after a bet is always `old balance - bet amount + win amount`.
 
## Running locally
 
Requirements: [.NET 8 SDK](https://dotnet.microsoft.com/download).
 
```bash
dotnet build
dotnet run --project src/Betty.Wallet.Console
```
 
(Adjust the project path above if your local folder layout differs — the solution is organized as `src/Betty.Wallet.Domain`, `src/Betty.Wallet.Application`, `src/Betty.Wallet.Console`, and `tests/Betty.Wallet.Tests`.)
 
## Running tests
 
```bash
dotnet test
```
 
The suite covers the game engine, wallet domain logic, command parsing, application orchestration, and full end-to-end command sequences — see Testing strategy below.
 
## Design
 
The solution is split into four projects with a strict, one-directional reference chain:
 
```text
Betty.Wallet.Console
        |
Betty.Wallet.Application
        |
Betty.Wallet.Domain
```
 
**Domain** owns the core rules and has no dependencies on anything else: `Wallet` (balance, deposits, withdrawals, bet placement/settlement — no public balance setter), `Game` (the $1–$10 bet range rule and outcome generation), `GameResult`/`GameOutcome`, and an `IRandomProvider` abstraction so the game engine never depends on a concrete source of randomness.
 
**Application** orchestrates the domain for a single logical operation and owns all user-facing validation: `WalletGameService` wraps a `Wallet` and a `Game`, exposes `Deposit`/`Withdraw`/`PlaceBet`, and returns an `OperationResult` describing what happened. `SystemRandomProvider` (the production `IRandomProvider` implementation, backed by `Random.Shared`) also lives here. `WalletGameService`'s parameterless constructor self-wires these production dependencies via constructor chaining, so `Console` never needs a reference to `Domain` at all.
 
**Console** is the composition root and presentation layer: `CommandParser` turns raw input lines into structured commands, `OutputFormatter` turns results back into user-facing text, and `Program.cs` is a thin read/parse/dispatch/print loop with no domain logic of its own.
 
**Tests** references everything and is organized to mirror the layer being tested (domain, application, console/parsing, plus cross-cutting application-level sequence tests).
 
## Other elaborations
 
- Money is represented as `decimal` throughout — never `double` — to avoid binary floating-point representation error in financial calculations.
- Input amounts with more than two decimal places are rejected outright at the parsing layer rather than silently rounded, since silently altering a user-supplied monetary amount is a worse outcome than asking them to re-enter it.
- Computed winnings are rounded to two decimal places using `MidpointRounding.AwayFromZero`.
- The wallet balance can never go negative; this is enforced as a domain invariant, not just a UI-level check.
- Withdrawing the full balance down to exactly $0 is valid.
- Commands are case-insensitive and tolerant of leading/trailing/extra internal whitespace.
- Blank input is treated as a silent no-op (re-prompt) rather than an error, for smoother interactive use.
- Structurally malformed amounts (currency symbols, thousands separators, scientific notation, a leading `+`) are rejected with one consistent parsing error rather than being partially interpreted.
- Validation is fail-fast on "amount must be positive," but once an amount is structurally valid, all remaining applicable rule violations for that operation are collected and shown together (most relevant to bets, which can simultaneously be out of the $1–$10 range and exceed the available balance).
## Financial handling
 
All monetary values are `decimal`. Parsing and formatting both use `CultureInfo.InvariantCulture` to avoid locale-dependent decimal separators or currency symbols leaking into behavior. Output is formatted manually as `"$" + amount.ToString("F2", CultureInfo.InvariantCulture)` rather than the `"C"` standard format specifier, because `"C"` under `InvariantCulture` prints a generic currency symbol (`¤`), not `$`. Rounding of computed winnings uses `MidpointRounding.AwayFromZero` applied to two decimal places, kept consistent with the two-decimal-place input restriction described above.
 
## Testing strategy
 
The suite favors deterministic, behavior-focused tests over incidental coverage:
 
- **Game tests** verify each outcome bucket (lose / small win / large win) and multiplier boundary using a hand-rolled `StubRandomProvider` test double that returns a fixed, queued sequence of draws — no mocking library, and no flaky statistical/probability-based tests.
- **Wallet tests** cover deposit, withdrawal, bet placement, and bet settlement for both success and failure paths, including an explicit check that a failed operation never mutates the balance.
- **Command parser tests** cover valid commands in all casings and whitespace variants, missing/extra arguments, unknown commands, and the full range of malformed amount formats.
- **Application service tests** cover `WalletGameService` orchestration: validation collection (a single violation vs. two simultaneous violations), and win/lose outcomes driven deterministically through the same stub-provider technique used in the game tests.
- **Application-level sequence tests** drive `CommandParser` and `WalletGameService` together through a full multi-command session without a real terminal, asserting the balance after every step, and a second sequence proving a failed operation mid-session doesn't corrupt subsequent state.
## Architecture decisions
 
- **Four projects instead of one.** The reference direction (`Console -> Application -> Domain`) is enforced by the project structure itself, not just convention — `Console` physically cannot reference `Domain` without adding a project reference, which keeps all orchestration and validation funneled through `Application`.
- **`IRandomProvider` instead of coupling directly to `System.Random`.** This is the one abstraction introduced purely for testability — it lets the game engine be exercised with a fixed, known sequence of "random" draws instead of relying on statistical tests.
- **No `Money` value object.** Deliberately not introduced: a single `decimal` type consistently formatted and validated at the boundaries covers this assignment's needs without adding a wrapper type whose main justification would be architectural symmetry rather than a real problem it solves.
- **`OperationResult` never exposes `Domain` types.** It exposes an `Application`-owned `BetOutcome` enum instead of `Domain.GameOutcome`, so that reading a result from `Console` can't accidentally reopen a dependency on `Domain`.
- **Classes are `sealed` by default.** None of the concrete types in this solution were designed with inheritance in mind; sealing communicates that directly instead of leaving extensibility ambiguous.
