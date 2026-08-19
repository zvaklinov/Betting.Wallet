using Betty.Wallet.Application;
using Betty.Wallet.Console;
using Console = System.Console;

var service = new WalletGameService();

Console.WriteLine(OutputFormatter.WelcomeBanner(service));
Console.WriteLine();

while (true)
{
    Console.Write("> ");
    string? line = Console.ReadLine();

    if (line is null)
    {
        break;
    }

    if (string.IsNullOrWhiteSpace(line))
    {
        continue;
    }

    ParseResult parseResult = CommandParser.Parse(line);

    if (!parseResult.Success)
    {
        Console.WriteLine($"Error: {parseResult.Error}");
        continue;
    }

    if (parseResult.Kind == CommandKind.Exit)
    {
        Console.WriteLine(OutputFormatter.Farewell);
        break;
    }

    OperationResult result = parseResult.Kind switch
    {
        CommandKind.Deposit => service.Deposit(parseResult.Amount),
        CommandKind.Withdraw => service.Withdraw(parseResult.Amount),
        CommandKind.Bet => service.PlaceBet(parseResult.Amount),
        _ => throw new InvalidOperationException($"Unhandled command kind: {parseResult.Kind}")
    };

    Console.WriteLine(result.Success
        ? OutputFormatter.FormatSuccess(parseResult.Kind, parseResult.Amount, result)
        : OutputFormatter.FormatErrors(result.Errors));
}