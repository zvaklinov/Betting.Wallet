using Betty.Wallet.Domain;

public class GameTests
{
    [Theory]
    [InlineData(0.0)]
    [InlineData(0.25)]
    [InlineData(0.4999)]
    public void Play_BelowPointFive_ReturnsLose(double bucketDraw)
    {
        var game = new Game(new StubRandomProvider(bucketDraw));

        var result = game.Play(betAmount: 5.00m);

        Assert.Equal(GameOutcome.Lose, result.Outcome);
        Assert.Equal(0m, result.Multiplier);
        Assert.Equal(0m, result.WinAmount);
    }

    [Fact]
    public void Play_SmallWin_LowerBoundary_MultiplierIsExactlyOne()
    {
        var game = new Game(new StubRandomProvider(0.5, 0.0));

        var result = game.Play(betAmount: 10.00m);

        Assert.Equal(GameOutcome.SmallWin, result.Outcome);
        Assert.Equal(1.0m, result.Multiplier);
        Assert.Equal(10.00m, result.WinAmount);
    }

    [Fact]
    public void Play_SmallWin_MidRange_ComputesExpectedAmount()
    {
        var game = new Game(new StubRandomProvider(0.7, 0.5));

        var result = game.Play(betAmount: 10.00m);

        Assert.Equal(1.5m, result.Multiplier);
        Assert.Equal(15.00m, result.WinAmount);
    }

    [Fact]
    public void Play_SmallWin_UpperBoundary_ApproachesButNeverReachesTwo()
    {
        var game = new Game(new StubRandomProvider(0.89, 0.999999));

        var result = game.Play(betAmount: 10.00m);

        Assert.Equal(GameOutcome.SmallWin, result.Outcome);
        Assert.InRange(result.Multiplier, 1.99m, 1.999999m);
    }

    [Fact]
    public void Play_LargeWin_UpperBoundary_MultiplierIsExactlyTen()
    {
        var game = new Game(new StubRandomProvider(0.9, 0.0));

        var result = game.Play(betAmount: 2.00m);

        Assert.Equal(GameOutcome.LargeWin, result.Outcome);
        Assert.Equal(10.0m, result.Multiplier);
        Assert.Equal(20.00m, result.WinAmount);
    }

    [Fact]
    public void Play_LargeWin_LowerBoundary_ApproachesButNeverReachesTwo()
    {
        var game = new Game(new StubRandomProvider(0.95, 0.999999));

        var result = game.Play(betAmount: 10.00m);

        Assert.Equal(GameOutcome.LargeWin, result.Outcome);
        Assert.InRange(result.Multiplier, 2.000001m, 2.01m);
    }

    [Fact]
    public void Play_LargeWin_NeverProducesMultiplierOfExactlyTwo()
    {
        var largeWin = new Game(new StubRandomProvider(0.9, 0.999999));

        var result = largeWin.Play(betAmount: 1.00m);

        Assert.Equal(GameOutcome.LargeWin, result.Outcome);
        Assert.NotEqual(2.0m, result.Multiplier);
        Assert.True(result.Multiplier > 2.0m);
    }

    [Fact]
    public void Play_WinAmount_RoundsAwayFromZeroAtMidpoint()
    {
        var game = new Game(new StubRandomProvider(0.6, 0.125));

        var result = game.Play(betAmount: 1.00m);

        Assert.Equal(1.125m, result.Multiplier);
        Assert.Equal(1.13m, result.WinAmount);
    }

    [Theory]
    [InlineData(1.00, true)]
    [InlineData(10.00, true)]
    [InlineData(5.50, true)]
    [InlineData(0.99, false)]
    [InlineData(10.01, false)]
    [InlineData(0, false)]
    public void IsValidBetAmount_ChecksInclusiveRange(decimal amount, bool expected)
    {
        var game = new Game(new StubRandomProvider());

        Assert.Equal(expected, game.IsValidBetAmount(amount));
    }
}