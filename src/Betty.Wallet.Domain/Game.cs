namespace Betty.Wallet.Domain;

public sealed class Game
{
    private readonly IRandomProvider _randomProvider;

    public decimal MinBet { get; }
    public decimal MaxBet { get; }

    public Game(IRandomProvider randomProvider, decimal minBet = 1.0m, decimal maxBet = 10.0m)
    {
        _randomProvider = randomProvider ?? throw new ArgumentNullException(nameof(randomProvider));
        MinBet = minBet;
        MaxBet = maxBet;
    }

    public bool IsValidBetAmount(decimal amount)
    {
        return amount >= MinBet && amount <= MaxBet;
    }

    public GameResult Play(decimal betAmount)
    {
        double bucketDraw = _randomProvider.NextDouble();

        GameOutcome outcome = bucketDraw switch
        {
            < 0.5 => GameOutcome.Lose,
            < 0.9 => GameOutcome.SmallWin,
            _ => GameOutcome.LargeWin
        };

        if (outcome == GameOutcome.Lose)
        {
            return new GameResult(GameOutcome.Lose, Multiplier: 0m, WinAmount: 0m);
        }

        double multiplierDraw = _randomProvider.NextDouble();

        decimal multiplier = outcome == GameOutcome.SmallWin
            ? 1.0m + (decimal)multiplierDraw * 1.0m   // achievable range: [1.0, 2.0)
            : 10.0m - (decimal)multiplierDraw * 8.0m;  // achievable range: (2.0, 10.0]

        decimal rawWin = betAmount * multiplier;
        decimal winAmount = Math.Round(rawWin, 2, MidpointRounding.AwayFromZero);

        return new GameResult(outcome, multiplier, winAmount);
    }
}