using Betty.Wallet.Domain;
using System.Globalization;
using WalletEntity = Betty.Wallet.Domain.Wallet;

namespace Betty.Wallet.Application;

public sealed class WalletGameService
{
    private readonly WalletEntity _wallet;
    private readonly Game _game;

    public WalletGameService(WalletEntity wallet, Game game)
    {
        _wallet = wallet ?? throw new ArgumentNullException(nameof(wallet));
        _game = game ?? throw new ArgumentNullException(nameof(game));
    }

    public WalletGameService() : this(new WalletEntity(), new Game(new SystemRandomProvider()))
    {
    }

    public decimal Balance => _wallet.Balance;
    public decimal MinBet => _game.MinBet;
    public decimal MaxBet => _game.MaxBet;

    public OperationResult Deposit(decimal amount)
    {
        if (amount <= 0)
        {
            return OperationResult.Failed(new[] { "Deposit amount must be positive." }, _wallet.Balance);
        }

        _wallet.Deposit(amount);
        return OperationResult.Ok(_wallet.Balance);
    }

    public OperationResult Withdraw(decimal amount)
    {
        if (amount <= 0)
        {
            return OperationResult.Failed(new[] { "Withdrawal amount must be positive." }, _wallet.Balance);
        }

        if (amount > _wallet.Balance)
        {
            return OperationResult.Failed(
                new[] { $"Insufficient funds: current balance is {_wallet.Balance.ToString(CultureInfo.InvariantCulture)}." },
                _wallet.Balance);
        }

        _wallet.Withdraw(amount);
        return OperationResult.Ok(_wallet.Balance);
    }

    public OperationResult PlaceBet(decimal amount)
    {
        if (amount <= 0)
        {
            return OperationResult.Failed(new[] { "Bet amount must be positive." }, _wallet.Balance);
        }

        var errors = new List<string>();

        if (!_game.IsValidBetAmount(amount))
        {
            errors.Add(
                $"Bet must be between {_game.MinBet.ToString(CultureInfo.InvariantCulture)} " +
                $"and {_game.MaxBet.ToString(CultureInfo.InvariantCulture)}.");
        }

        if (amount > _wallet.Balance)
        {
            errors.Add($"Insufficient funds: current balance is {_wallet.Balance.ToString(CultureInfo.InvariantCulture)}.");
        }

        if (errors.Count > 0)
        {
            return OperationResult.Failed(errors, _wallet.Balance);
        }

        _wallet.PlaceBet(amount);
        GameResult gameResult = _game.Play(amount);
        _wallet.SettleBet(gameResult.WinAmount);

        return OperationResult.OkFromBet(_wallet.Balance, MapOutcome(gameResult.Outcome), gameResult.WinAmount);
    }

    private static BetOutcome MapOutcome(GameOutcome outcome) => outcome switch
    {
        GameOutcome.Lose => BetOutcome.Lose,
        GameOutcome.SmallWin => BetOutcome.SmallWin,
        GameOutcome.LargeWin => BetOutcome.LargeWin,
        _ => throw new ArgumentOutOfRangeException(nameof(outcome))
    };
}