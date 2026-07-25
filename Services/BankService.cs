using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using BankATMSimulator.Models;

namespace BankATMSimulator.Services
{
    /// <summary>
    /// All core banking logic lives here: account creation, login, deposit,
    /// withdraw, transfer, statements, and JSON file persistence.
    /// Program.cs only handles the menu / console I/O.
    /// </summary>
    public class BankService
    {
        private readonly string _dataFilePath;
        private List<Account> _accounts;

        private const int MaxPinAttempts = 3;
        private const decimal MinOpeningDeposit = 500m;

        public BankService(string dataFilePath = "Data/accounts.json")
        {
            _dataFilePath = dataFilePath;
            _accounts = LoadAccounts();
        }

        // ---------- Persistence ----------

        private List<Account> LoadAccounts()
        {
            var dir = Path.GetDirectoryName(_dataFilePath);
            if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
                Directory.CreateDirectory(dir);

            if (!File.Exists(_dataFilePath))
                return new List<Account>();

            try
            {
                var json = File.ReadAllText(_dataFilePath);
                if (string.IsNullOrWhiteSpace(json))
                    return new List<Account>();

                return JsonSerializer.Deserialize<List<Account>>(json) ?? new List<Account>();
            }
            catch (JsonException)
            {
                // Corrupt data file: start fresh instead of crashing the app.
                return new List<Account>();
            }
        }

        private void SaveAccounts()
        {
            var options = new JsonSerializerOptions { WriteIndented = true };
            var json = JsonSerializer.Serialize(_accounts, options);
            File.WriteAllText(_dataFilePath, json);
        }

        // ---------- Helpers ----------

        private static string HashPin(string pin)
        {
            byte[] bytes = SHA256.HashData(Encoding.UTF8.GetBytes(pin));
            return Convert.ToHexString(bytes);
        }

        private string GenerateAccountNumber()
        {
            // Simple readable format: BD + 8 digit random number, unique among existing accounts.
            var rng = new Random();
            string number;
            do
            {
                number = "BD" + rng.Next(10_000_000, 99_999_999);
            } while (_accounts.Any(a => a.AccountNumber == number));

            return number;
        }

        private static bool IsValidPin(string pin) =>
            pin.Length == 4 && pin.All(char.IsDigit);

        // ---------- Account creation ----------

        public (bool Success, string Message, Account? Account) CreateAccount(string holderName, string pin, decimal openingDeposit)
        {
            if (string.IsNullOrWhiteSpace(holderName))
                return (false, "Holder name cannot be empty.", null);

            if (!IsValidPin(pin))
                return (false, "PIN must be exactly 4 digits.", null);

            if (openingDeposit < MinOpeningDeposit)
                return (false, $"Minimum opening deposit is {MinOpeningDeposit:C}.", null);

            var account = new Account
            {
                AccountNumber = GenerateAccountNumber(),
                HolderName = holderName.Trim(),
                PinHash = HashPin(pin),
                Balance = openingDeposit
            };

            account.Transactions.Add(new Transaction
            {
                Type = "Opening Deposit",
                Amount = openingDeposit,
                BalanceAfter = account.Balance,
                Note = "Account opened"
            });

            _accounts.Add(account);
            SaveAccounts();

            return (true, "Account created successfully.", account);
        }

        // ---------- Login ----------

        public (bool Success, string Message, Account? Account) Login(string accountNumber, string pin)
        {
            var account = _accounts.FirstOrDefault(a => a.AccountNumber == accountNumber);
            if (account is null)
                return (false, "Account not found.", null);

            if (account.IsLocked)
                return (false, "This account is locked due to too many failed PIN attempts. Contact support.", null);

            if (account.PinHash != HashPin(pin))
            {
                account.FailedPinAttempts++;
                if (account.FailedPinAttempts >= MaxPinAttempts)
                {
                    account.IsLocked = true;
                    SaveAccounts();
                    return (false, "Incorrect PIN. Account is now locked after 3 failed attempts.", null);
                }

                SaveAccounts();
                int remaining = MaxPinAttempts - account.FailedPinAttempts;
                return (false, $"Incorrect PIN. {remaining} attempt(s) remaining.", null);
            }

            account.FailedPinAttempts = 0;
            SaveAccounts();
            return (true, "Login successful.", account);
        }

        // ---------- Transactions ----------

        public (bool Success, string Message) Deposit(Account account, decimal amount)
        {
            if (amount <= 0)
                return (false, "Deposit amount must be positive.");

            account.Balance += amount;
            account.Transactions.Add(new Transaction
            {
                Type = "Deposit",
                Amount = amount,
                BalanceAfter = account.Balance
            });

            SaveAccounts();
            return (true, $"Deposited {amount:C}. New balance: {account.Balance:C}");
        }

        public (bool Success, string Message) Withdraw(Account account, decimal amount)
        {
            if (amount <= 0)
                return (false, "Withdrawal amount must be positive.");

            if (amount > account.Balance)
                return (false, "Insufficient balance.");

            account.Balance -= amount;
            account.Transactions.Add(new Transaction
            {
                Type = "Withdraw",
                Amount = amount,
                BalanceAfter = account.Balance
            });

            SaveAccounts();
            return (true, $"Withdrew {amount:C}. New balance: {account.Balance:C}");
        }

        public (bool Success, string Message) Transfer(Account fromAccount, string toAccountNumber, decimal amount)
        {
            if (amount <= 0)
                return (false, "Transfer amount must be positive.");

            if (fromAccount.AccountNumber == toAccountNumber)
                return (false, "Cannot transfer to the same account.");

            var toAccount = _accounts.FirstOrDefault(a => a.AccountNumber == toAccountNumber);
            if (toAccount is null)
                return (false, "Destination account not found.");

            if (amount > fromAccount.Balance)
                return (false, "Insufficient balance.");

            fromAccount.Balance -= amount;
            toAccount.Balance += amount;

            fromAccount.Transactions.Add(new Transaction
            {
                Type = "Transfer-Out",
                Amount = amount,
                BalanceAfter = fromAccount.Balance,
                Note = $"To {toAccount.AccountNumber}"
            });

            toAccount.Transactions.Add(new Transaction
            {
                Type = "Transfer-In",
                Amount = amount,
                BalanceAfter = toAccount.Balance,
                Note = $"From {fromAccount.AccountNumber}"
            });

            SaveAccounts();
            return (true, $"Transferred {amount:C} to {toAccount.AccountNumber}. New balance: {fromAccount.Balance:C}");
        }

        public (bool Success, string Message) ChangePin(Account account, string oldPin, string newPin)
        {
            if (account.PinHash != HashPin(oldPin))
                return (false, "Current PIN is incorrect.");

            if (!IsValidPin(newPin))
                return (false, "New PIN must be exactly 4 digits.");

            account.PinHash = HashPin(newPin);
            SaveAccounts();
            return (true, "PIN changed successfully.");
        }

        public List<Transaction> GetStatement(Account account, int lastN = 10)
        {
            return account.Transactions
                .OrderByDescending(t => t.Date)
                .Take(lastN)
                .ToList();
        }
    }
}
