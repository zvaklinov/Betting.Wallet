using System.Globalization;

namespace Betty.Wallet.Domain;

public sealed class Wallet
{
    public decimal Balance { get; private set; }

    public void Deposit(decimal amount) => Credit(amount, allowZero: false);

    public void Withdraw(decimal amount) => Debit(amount);

    public void PlaceBet(decimal amount) => Debit(amount);

    public void SettleBet(decimal amount) => Credit(amount, allowZero: true);

    private void Credit(decimal amount, bool allowZero)
    {
        bool isInvalid = allowZero ? amount < 0 : amount <= 0;

        if (isInvalid)
        {
            throw new ArgumentOutOfRangeException(
                nameof(amount), amount,
                allowZero ? "Amount cannot be negative." : "Amount must be positive.");
        }

        Balance += amount;
    }

    private void Debit(decimal amount)
    {
        if (amount <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(amount), amount, "Amount must be positive.");
        }

        if (amount > Balance)
        {
            throw new InsufficientFundsException(
                $"Cannot debit {amount.ToString(CultureInfo.InvariantCulture)}; " +
                $"current balance is {Balance.ToString(CultureInfo.InvariantCulture)}.");
        }

        Balance -= amount;
    }
}