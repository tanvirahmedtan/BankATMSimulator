namespace BankATMSimulator.Models
{
    /// <summary>
    /// Represents a single ledger entry against an account (deposit, withdraw, transfer, etc).
    /// </summary>
    public class Transaction
    {
        public DateTime Date { get; set; } = DateTime.Now;
        public string Type { get; set; } = string.Empty;   // Deposit, Withdraw, Transfer-Out, Transfer-In
        public decimal Amount { get; set; }
        public decimal BalanceAfter { get; set; }
        public string Note { get; set; } = string.Empty;

        public override string ToString()
        {
            return $"{Date:yyyy-MM-dd HH:mm:ss}  {Type,-12}  {Amount,10:C}  Balance: {BalanceAfter,10:C}  {Note}";
        }
    }
}
