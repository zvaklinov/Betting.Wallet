using Betty.Wallet.Domain;

namespace Betty.Wallet.Application;

public sealed class SystemRandomProvider : IRandomProvider
{
    public double NextDouble() => Random.Shared.NextDouble();
}