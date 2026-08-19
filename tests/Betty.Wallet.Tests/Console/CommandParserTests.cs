using Betty.Wallet.Console;

namespace Betty.Wallet.Tests.Console;

public class CommandParserTests
{
    [Theory]
    [InlineData("deposit 10", CommandKind.Deposit, 10)]
    [InlineData("DEPOSIT 10", CommandKind.Deposit, 10)]
    [InlineData("  deposit   10  ", CommandKind.Deposit, 10)]
    [InlineData("withdraw 5.50", CommandKind.Withdraw, 5.50)]
    [InlineData("bet 3", CommandKind.Bet, 3)]
    [InlineData("Bet 3", CommandKind.Bet, 3)]
    public void Parse_ValidAmountCommand_Succeeds(string input, CommandKind expectedKind, decimal expectedAmount)
    {
        var result = CommandParser.Parse(input);

        Assert.True(result.Success);
        Assert.Equal(expectedKind, result.Kind);
        Assert.Equal(expectedAmount, result.Amount);
    }

    [Theory]
    [InlineData("exit")]
    [InlineData("EXIT")]
    [InlineData("  exit  ")]
    public void Parse_Exit_Succeeds(string input)
    {
        var result = CommandParser.Parse(input);

        Assert.True(result.Success);
        Assert.Equal(CommandKind.Exit, result.Kind);
    }

    [Fact]
    public void Parse_ExitWithExtraArgument_Fails()
    {
        Assert.False(CommandParser.Parse("exit now").Success);
    }

    [Fact]
    public void Parse_UnknownCommand_Fails()
    {
        Assert.False(CommandParser.Parse("transfer 10").Success);
    }

    [Fact]
    public void Parse_MissingAmount_Fails()
    {
        Assert.False(CommandParser.Parse("deposit").Success);
    }

    [Fact]
    public void Parse_TooManyArguments_Fails()
    {
        Assert.False(CommandParser.Parse("deposit 10 20").Success);
    }

    [Theory]
    [InlineData("deposit $10")]
    [InlineData("deposit 1,000")]
    [InlineData("deposit 1e5")]
    [InlineData("deposit +5")]
    [InlineData("deposit 10.")]
    [InlineData("deposit 10.123")]
    [InlineData("deposit abc")]
    public void Parse_MalformedAmount_Fails(string input)
    {
        Assert.False(CommandParser.Parse(input).Success);
    }

    [Fact]
    public void Parse_NegativeAmount_ParsesStructurally()
    {
        var result = CommandParser.Parse("deposit -5");

        Assert.True(result.Success);
        Assert.Equal(-5m, result.Amount);
    }

    [Fact]
    public void Parse_ZeroAmount_ParsesStructurally()
    {
        var result = CommandParser.Parse("bet 0");

        Assert.True(result.Success);
        Assert.Equal(0m, result.Amount);
    }
}