
namespace Betty.Wallet.Domain
{
    /// <summary>
    /// The outcome of one round. For Lose, Multiplier and WinAmount are both 0.
    /// </summary>
    public sealed record GameResult(GameOutcome Outcome, decimal Multiplier, decimal WinAmount);
}
