namespace Betty.Wallet.Console;

public sealed class ParseResult
{
    public bool Success { get; }
    public string? Error { get; }
    public CommandKind Kind { get; }
    public decimal Amount { get; }

    private ParseResult(bool success, string? error, CommandKind kind, decimal amount)
    {
        Success = success;
        Error = error;
        Kind = kind;
        Amount = amount;
    }

    public static ParseResult Ok(CommandKind kind, decimal amount = 0m) => new(true, null, kind, amount);

    public static ParseResult Fail(string error) => new(false, error, default, 0m);
}