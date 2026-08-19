using System.Globalization;
using System.Text.RegularExpressions;

namespace Betty.Wallet.Console;

public static class CommandParser
{
    private static readonly Regex AmountPattern = new(@"^-?\d+(\.\d{1,2})?$", RegexOptions.Compiled);

    public static ParseResult Parse(string rawInput)
    {
        string[] tokens = rawInput.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries);

        if (tokens.Length == 0)
        {
            return ParseResult.Fail("No command entered.");
        }

        string verb = tokens[0];

        if (string.Equals(verb, "exit", StringComparison.OrdinalIgnoreCase))
        {
            return tokens.Length == 1
                ? ParseResult.Ok(CommandKind.Exit)
                : ParseResult.Fail("'exit' does not take any arguments.");
        }

        CommandKind? kind = verb.ToLowerInvariant() switch
        {
            "deposit" => CommandKind.Deposit,
            "withdraw" => CommandKind.Withdraw,
            "bet" => CommandKind.Bet,
            _ => null
        };

        if (kind is null)
        {
            return ParseResult.Fail($"Unknown command: '{verb}'.");
        }

        if (tokens.Length != 2)
        {
            return ParseResult.Fail($"'{verb}' requires exactly one amount argument.");
        }

        string amountToken = tokens[1];

        if (!AmountPattern.IsMatch(amountToken))
        {
            return ParseResult.Fail(
                $"'{amountToken}' is not a valid amount. Use a plain number with up to 2 decimal places.");
        }


        if (!decimal.TryParse(amountToken, NumberStyles.AllowLeadingSign | NumberStyles.AllowDecimalPoint,
                CultureInfo.InvariantCulture, out decimal amount))
        {
            return ParseResult.Fail($"'{amountToken}' is not a valid amount.");
        }

        return ParseResult.Ok(kind.Value, amount);
    }
}