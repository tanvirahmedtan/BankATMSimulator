namespace BankATMSimulator.Models
{
    /// <summary>
    /// Represents a bank account. The PIN is stored as a SHA-256 hash, never in plain text.
    /// </summary>
    public class Account
    {
        public string AccountNumber { get; set; } = string.Empty;
        public string HolderName { get; set; } = string.Empty;
        public string PinHash { get; set; } = string.Empty;
        public decimal Balance { get; set; }
        public List<Transaction> Transactions { get; set; } = new();
        public bool IsLocked { get; set; } = false;
        public int FailedPinAttempts { get; set; } = 0;
        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}
