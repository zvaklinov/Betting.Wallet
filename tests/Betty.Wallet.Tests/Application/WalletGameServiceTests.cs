using Betty.Wallet.Application;
using Betty.Wallet.Domain;
using WalletEntity = Betty.Wallet.Domain.Wallet;

namespace Betty.Wallet.Tests.Application;

public class WalletGameServiceTests
{
    [Fact]
    public void DefaultConstructor_StartsWithZeroBalance()
    {
        var service = new WalletGameService();

        Assert.Equal(0m, service.Balance);
    }

    [Fact]
    public void DefaultConstructor_ExposesStandardBetRange()
    {
        var service = new WalletGameService();

        Assert.Equal(1.0m, service.MinBet);
        Assert.Equal(10.0m, service.MaxBet);
    }

    [Fact]
    public void Deposit_PositiveAmount_IncreasesBalanceAndSucceeds()
    {
        var service = new WalletGameService();

        var result = service.Deposit(10m);

        Assert.True(result.Success);
        Assert.Empty(result.Errors);
        Assert.Equal(10m, result.Balance);
        Assert.Equal(10m, service.Balance);
    }

    [Fact]
    public void Deposit_Zero_FailsAndDoesNotChangeBalance()
    {
        var service = new WalletGameService();

        var result = service.Deposit(0m);

        Assert.False(result.Success);
        Assert.NotEmpty(result.Errors);
        Assert.Equal(0m, service.Balance);
    }

    [Fact]
    public void Deposit_Negative_FailsAndDoesNotChangeBalance()
    {
        var service = new WalletGameService();

        var result = service.Deposit(-5m);

        Assert.False(result.Success);
        Assert.NotEmpty(result.Errors);
        Assert.Equal(0m, service.Balance);
    }

    [Fact]
    public void Withdraw_ValidAmount_DecreasesBalanceAndSucceeds()
    {
        var service = new WalletGameService();
        service.Deposit(20m);

        var result = service.Withdraw(15m);

        Assert.True(result.Success);
        Assert.Equal(5m, result.Balance);
        Assert.Equal(5m, service.Balance);
    }

    [Fact]
    public void Withdraw_FullBalance_SucceedsAndLeavesZero()
    {
        var service = new WalletGameService();
        service.Deposit(20m);

        var result = service.Withdraw(20m);

        Assert.True(result.Success);
        Assert.Equal(0m, service.Balance);
    }

    [Fact]
    public void Withdraw_ExceedsBalance_FailsAndDoesNotChangeBalance()
    {
        var service = new WalletGameService();
        service.Deposit(10m);

        var result = service.Withdraw(15m);

        Assert.False(result.Success);
        Assert.NotEmpty(result.Errors);
        Assert.Equal(10m, service.Balance);
    }

    [Fact]
    public void Withdraw_Zero_FailsAndDoesNotChangeBalance()
    {
        var service = new WalletGameService();
        service.Deposit(10m);

        var result = service.Withdraw(0m);

        Assert.False(result.Success);
        Assert.Equal(10m, service.Balance);
    }

    [Fact]
    public void PlaceBet_Zero_FailsFastWithSingleError()
    {
        var service = new WalletGameService();
        service.Deposit(10m);

        var result = service.PlaceBet(0m);

        Assert.False(result.Success);
        Assert.Single(result.Errors);
        Assert.Equal(10m, service.Balance);
    }

    [Fact]
    public void PlaceBet_WithinRangeButExceedsBalance_ReturnsSingleInsufficientFundsError()
    {
        var service = new WalletGameService();
        service.Deposit(5m);

        var result = service.PlaceBet(8m);

        Assert.False(result.Success);
        Assert.Single(result.Errors);
        Assert.Equal(5m, service.Balance);
    }

    [Fact]
    public void PlaceBet_AboveMaxAndExceedsBalance_ReturnsBothViolations()
    {
        var service = new WalletGameService();

        var result = service.PlaceBet(15m);

        Assert.False(result.Success);
        Assert.Equal(2, result.Errors.Count);
        Assert.Equal(0m, service.Balance);
    }

    [Fact]
    public void PlaceBet_BelowMinAndExceedsBalance_ReturnsBothViolations()
    {
        var service = new WalletGameService();

        var result = service.PlaceBet(0.50m);

        Assert.False(result.Success);
        Assert.Equal(2, result.Errors.Count);
        Assert.Equal(0m, service.Balance);
    }

    [Fact]
    public void PlaceBet_Lose_DecreasesBalanceByBetAmount()
    {
        var randomProvider = new StubRandomProvider(0.0, 0.0); // bucket draw < 0.5 -> Lose
        var service = new WalletGameService(new WalletEntity(), new Game(randomProvider));
        service.Deposit(10m);

        var result = service.PlaceBet(5m);

        Assert.True(result.Success);
        Assert.Equal(BetOutcome.Lose, result.Outcome);
        Assert.Equal(0m, result.WinAmount);
        Assert.Equal(5m, service.Balance);
    }

    [Fact]
    public void PlaceBet_SmallWin_AppliesFormulaCorrectly()
    {
        // bucket draw 0.5 -> SmallWin bucket; multiplier draw 0.0 -> multiplier = 1.0 + 0.0 = 1.0
        var randomProvider = new StubRandomProvider(0.5, 0.0);
        var service = new WalletGameService(new WalletEntity(), new Game(randomProvider));
        service.Deposit(10m);

        var result = service.PlaceBet(5m);

        Assert.True(result.Success);
        Assert.Equal(BetOutcome.SmallWin, result.Outcome);
        Assert.Equal(5.00m, result.WinAmount);
        Assert.Equal(10m, service.Balance);
    }

    [Fact]
    public void PlaceBet_LargeWin_AppliesFormulaCorrectly()
    {
        // bucket draw 0.9 -> LargeWin bucket; multiplier draw 0.0 -> multiplier = 10.0 - 0.0 = 10.0
        var randomProvider = new StubRandomProvider(0.9, 0.0);
        var service = new WalletGameService(new WalletEntity(), new Game(randomProvider));
        service.Deposit(10m);

        var result = service.PlaceBet(5m);

        Assert.True(result.Success);
        Assert.Equal(BetOutcome.LargeWin, result.Outcome);
        Assert.Equal(50.00m, result.WinAmount);
        Assert.Equal(55m, service.Balance);
    }
}