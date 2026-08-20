using Betty.Wallet.Domain;

namespace Betty.Wallet.Tests.Domain;

internal sealed class StubRandomProvider : IRandomProvider
{
    private readonly Queue<double> _values;

    public StubRandomProvider(params double[] values) => _values = new Queue<double>(values);

    public double NextDouble() =>
        _values.Count > 0
            ? _values.Dequeue()
            : throw new InvalidOperationException("StubRandomProvider ran out of programmed values.");
}