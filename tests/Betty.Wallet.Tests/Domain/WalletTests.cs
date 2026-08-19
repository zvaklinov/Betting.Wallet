using Betty.Wallet.Domain;
using WalletEntity = Betty.Wallet.Domain.Wallet;

namespace Betty.Wallet.Tests.Domain;

public class WalletTests
{
    [Fact]
    public void NewWallet_StartsAtZeroBalance()
    {
        var wallet = new WalletEntity();

        Assert.Equal(0m, wallet.Balance);
    }

    [Fact]
    public void Deposit_PositiveAmount_IncreasesBalance()
    {
        var wallet = new WalletEntity();

        wallet.Deposit(10.50m);

        Assert.Equal(10.50m, wallet.Balance);
    }

    [Fact]
    public void Deposit_Twice_Accumulates()
    {
        var wallet = new WalletEntity();

        wallet.Deposit(10m);
        wallet.Deposit(5m);

        Assert.Equal(15m, wallet.Balance);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-0.01)]
    [InlineData(-100)]
    public void Deposit_ZeroOrNegative_ThrowsAndDoesNotChangeBalance(decimal amount)
    {
        var wallet = new WalletEntity();
        wallet.Deposit(20m);

        Assert.Throws<ArgumentOutOfRangeException>(() => wallet.Deposit(amount));
        Assert.Equal(20m, wallet.Balance);
    }

    [Fact]
    public void Withdraw_ValidAmount_DecreasesBalance()
    {
        var wallet = new WalletEntity();
        wallet.Deposit(20m);

        wallet.Withdraw(8m);

        Assert.Equal(12m, wallet.Balance);
    }

    [Fact]
    public void Withdraw_FullBalance_ResultsInZero()
    {
        var wallet = new WalletEntity();
        wallet.Deposit(15m);

        wallet.Withdraw(15m);

        Assert.Equal(0m, wallet.Balance);
    }

    [Fact]
    public void Withdraw_ExceedsBalance_ThrowsAndDoesNotChangeBalance()
    {
        var wallet = new WalletEntity();
        wallet.Deposit(10m);

        Assert.Throws<InsufficientFundsException>(() => wallet.Withdraw(10.01m));
        Assert.Equal(10m, wallet.Balance);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-5)]
    public void Withdraw_ZeroOrNegative_ThrowsAndDoesNotChangeBalance(decimal amount)
    {
        var wallet = new WalletEntity();
        wallet.Deposit(10m);

        Assert.Throws<ArgumentOutOfRangeException>(() => wallet.Withdraw(amount));
        Assert.Equal(10m, wallet.Balance);
    }

    [Fact]
    public void PlaceBet_ValidAmount_DecreasesBalance()
    {
        var wallet = new WalletEntity();
        wallet.Deposit(20m);

        wallet.PlaceBet(5m);

        Assert.Equal(15m, wallet.Balance);
    }

    [Fact]
    public void PlaceBet_ExceedsBalance_ThrowsAndDoesNotChangeBalance()
    {
        var wallet = new WalletEntity();
        wallet.Deposit(3m);

        Assert.Throws<InsufficientFundsException>(() => wallet.PlaceBet(5m));
        Assert.Equal(3m, wallet.Balance);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void PlaceBet_ZeroOrNegative_ThrowsAndDoesNotChangeBalance(decimal amount)
    {
        var wallet = new WalletEntity();
        wallet.Deposit(10m);

        Assert.Throws<ArgumentOutOfRangeException>(() => wallet.PlaceBet(amount));
        Assert.Equal(10m, wallet.Balance);
    }

    [Fact]
    public void SettleBet_PositiveAmount_IncreasesBalance()
    {
        var wallet = new WalletEntity();
        wallet.Deposit(10m);

        wallet.SettleBet(35.35m);

        Assert.Equal(45.35m, wallet.Balance);
    }

    [Fact]
    public void SettleBet_Zero_IsAllowedAndLeavesBalanceUnchanged()
    {
        var wallet = new WalletEntity();
        wallet.Deposit(10m);

        wallet.SettleBet(0m);

        Assert.Equal(10m, wallet.Balance);
    }

    [Fact]
    public void SettleBet_Negative_ThrowsAndDoesNotChangeBalance()
    {
        var wallet = new WalletEntity();
        wallet.Deposit(10m);

        Assert.Throws<ArgumentOutOfRangeException>(() => wallet.SettleBet(-0.01m));
        Assert.Equal(10m, wallet.Balance);
    }

    [Fact]
    public void Sequence_DepositBetWinWithdraw_ProducesExpectedFinalBalance()
    {
        var wallet = new WalletEntity();

        wallet.Deposit(10m);
        wallet.PlaceBet(5m);
        wallet.SettleBet(35.35m);
        wallet.Withdraw(2m);

        Assert.Equal(38.35m, wallet.Balance);
    }
}