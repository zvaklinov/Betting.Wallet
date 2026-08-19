namespace Betty.Wallet.Domain;

public sealed class InsufficientFundsException : InvalidOperationException
{
    public InsufficientFundsException(string message) : base(message)
    {
    }
}