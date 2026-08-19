using Betty.Wallet.Application;
using System.Globalization;

namespace Betty.Wallet.Console;

public static class OutputFormatter
{
    public const string Farewell = "Thank you for playing! Hope to see you again soon.";

    public static string FormatMoney(decimal amount) =>
        $"${amount.ToString("F2", CultureInfo.InvariantCulture)}";

    public static string WelcomeBanner(WalletGameService service)
    {
        return string.Join(Environment.NewLine,
            "Welcome to Betty Wallet!",
            "",
            "Available commands:",
            "  deposit <amount>   deposit funds",
            "  withdraw <amount>  withdraw funds",
            $"  bet <amount>       place a bet between {FormatMoney(service.MinBet)} and {FormatMoney(service.MaxBet)}",
            "  exit               quit",
            "",
            "All amounts must be positive numbers with at most 2 decimal places.",
            "",
            $"Your current balance is: {FormatMoney(service.Balance)}");
    }

    public static string FormatSuccess(CommandKind kind, decimal amount, OperationResult result) => kind switch
    {
        CommandKind.Deposit =>
            $"Your deposit of {FormatMoney(amount)} was successful. Your current balance is: {FormatMoney(result.Balance)}",
        CommandKind.Withdraw =>
            $"Your withdrawal of {FormatMoney(amount)} was successful. Your current balance is: {FormatMoney(result.Balance)}",
        CommandKind.Bet => FormatBetResult(amount, result),
        _ => $"Your current balance is: {FormatMoney(result.Balance)}"
    };

    private static string FormatBetResult(decimal betAmount, OperationResult result)
    {
        if (result.Outcome == BetOutcome.Lose)
        {
            return $"You lost {FormatMoney(betAmount)}. Your current balance is: {FormatMoney(result.Balance)}";
        }

        return $"You won {FormatMoney(result.WinAmount ?? 0m)}! Your current balance is: {FormatMoney(result.Balance)}";
    }

    public static string FormatErrors(IReadOnlyList<string> errors) =>
        string.Join(Environment.NewLine, errors.Select(e => $"Error: {e}"));
}