using Betty.Wallet.Application;
using Betty.Wallet.Console;
using Betty.Wallet.Domain;
using WalletEntity = Betty.Wallet.Domain.Wallet;

namespace Betty.Wallet.Tests.Application;

public class ApplicationSequenceTests
{
    [Fact]
    public void FullSession_DepositBetWithdrawExit_ProducesExpectedBalanceAtEachStep()
    {
        var randomProvider = new StubRandomProvider(0.0, 0.0);
        var service = new WalletGameService(new WalletEntity(), new Game(randomProvider));

        var commands = new[] { "deposit 10", "bet 5", "withdraw 2", "exit" };
        var balancesAfterEachCommand = new List<decimal>();

        foreach (var rawInput in commands)
        {
            var parseResult = CommandParser.Parse(rawInput);
            Assert.True(parseResult.Success, $"'{rawInput}' failed to parse: {parseResult.Error}");

            switch (parseResult.Kind)
            {
                case CommandKind.Deposit:
                    Assert.True(service.Deposit(parseResult.Amount).Success);
                    break;

                case CommandKind.Withdraw:
                    Assert.True(service.Withdraw(parseResult.Amount).Success);
                    break;

                case CommandKind.Bet:
                    Assert.True(service.PlaceBet(parseResult.Amount).Success);
                    break;

                case CommandKind.Exit:
                    break;
            }

            balancesAfterEachCommand.Add(service.Balance);
        }

        Assert.Equal(new[] { 10m, 5m, 3m, 3m }, balancesAfterEachCommand);
        Assert.Equal(3m, service.Balance);
    }

    [Fact]
    public void FullSession_FailedOperationMidSequence_DoesNotCorruptSubsequentState()
    {
        var service = new WalletGameService();

        var commands = new[] { "deposit 20", "withdraw 100", "withdraw 5" };
        var results = new List<OperationResult>();

        foreach (var rawInput in commands)
        {
            var parseResult = CommandParser.Parse(rawInput);
            Assert.True(parseResult.Success);

            var result = parseResult.Kind switch
            {
                CommandKind.Deposit => service.Deposit(parseResult.Amount),
                CommandKind.Withdraw => service.Withdraw(parseResult.Amount),
                CommandKind.Bet => service.PlaceBet(parseResult.Amount),
                _ => throw new InvalidOperationException("Unexpected command in this test.")
            };

            results.Add(result);
        }

        Assert.True(results[0].Success);
        Assert.False(results[1].Success);
        Assert.True(results[2].Success);

        Assert.Equal(15m, service.Balance);
    }
}