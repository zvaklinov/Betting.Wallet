namespace Betty.Wallet.Application
{
    public sealed class OperationResult
    {
        public bool Success { get; }
        public IReadOnlyList<string> Errors { get; }
        public decimal Balance { get; }
        public BetOutcome? Outcome { get; }
        public decimal? WinAmount { get; }

        private OperationResult(bool success, IReadOnlyList<string> errors, decimal balance, BetOutcome? outcome, decimal? winAmount)
        {
            Success = success;
            Errors = errors;
            Balance = balance;
            Outcome = outcome;
            WinAmount = winAmount;
        }

        public static OperationResult Ok(decimal balance) =>
            new(true, Array.Empty<string>(), balance, null, null);

        public static OperationResult OkFromBet(decimal balance, BetOutcome outcome, decimal winAmount) =>
            new(true, Array.Empty<string>(), balance, outcome, winAmount);

        public static OperationResult Failed(IReadOnlyList<string> errors, decimal balance) =>
            new(false, errors, balance, null, null);
    }
}
