using BankATMSimulator.Models;
using BankATMSimulator.Services;

namespace BankATMSimulator
{
    internal class Program
    {
        private static readonly BankService bank = new();

        private static void Main(string[] args)
        {
            Console.Title = "Bank / ATM Simulator";
            PrintBanner();

            bool running = true;
            while (running)
            {
                PrintMainMenu();
                switch (ReadMenuChoice(1, 3))
                {
                    case 1:
                        HandleCreateAccount();
                        break;
                    case 2:
                        HandleLogin();
                        break;
                    case 3:
                        running = false;
                        break;
                }
            }

            WriteLine("\nThank you for using Bank/ATM Simulator. Goodbye!", ConsoleColor.Cyan);
        }

        // ---------- Menus ----------

        private static void PrintBanner()
        {
            WriteLine(@"
  ____              _         _  _____ __  __ 
 |  _ \            | |       / \|_   _|  \/  |
 | |_) | __ _ _ __ | | __   / _ \ | | | |\/| |
 |  _ < / _` | '_ \| |/ /  / ___ \| | | |  | |
 | |_) | (_| | | | |   <  /_/   \_\_| |_|  |_|
 |____/ \__,_|_| |_|_|\_\   Bank / ATM Simulator
", ConsoleColor.Green);
        }

        private static void PrintMainMenu()
        {
            WriteLine("\n===== MAIN MENU =====", ConsoleColor.Yellow);
            Console.WriteLine("1. Create New Account");
            Console.WriteLine("2. Login");
            Console.WriteLine("3. Exit");
            Console.Write("Choose an option: ");
        }

        private static void PrintAccountMenu(Account account)
        {
            WriteLine($"\n===== WELCOME, {account.HolderName.ToUpper()} ({account.AccountNumber}) =====", ConsoleColor.Yellow);
            Console.WriteLine("1. Check Balance");
            Console.WriteLine("2. Deposit");
            Console.WriteLine("3. Withdraw");
            Console.WriteLine("4. Transfer to Another Account");
            Console.WriteLine("5. Mini Statement (last 10 transactions)");
            Console.WriteLine("6. Change PIN");
            Console.WriteLine("7. Logout");
            Console.Write("Choose an option: ");
        }

        // ---------- Handlers: main menu ----------

        private static void HandleCreateAccount()
        {
            WriteLine("\n-- Create New Account --", ConsoleColor.Cyan);

            Console.Write("Enter your full name: ");
            string name = Console.ReadLine() ?? string.Empty;

            string pin = ReadPin("Set a 4-digit PIN: ");
            decimal deposit = ReadDecimal("Enter opening deposit (minimum 500): ");

            var (success, message, account) = bank.CreateAccount(name, pin, deposit);

            if (success && account is not null)
            {
                WriteLine($"\n{message}", ConsoleColor.Green);
                WriteLine($"Your account number is: {account.AccountNumber}", ConsoleColor.Green);
                WriteLine("Please save this account number — you'll need it to log in.", ConsoleColor.Green);
            }
            else
            {
                WriteLine($"\n{message}", ConsoleColor.Red);
            }
        }

        private static void HandleLogin()
        {
            WriteLine("\n-- Login --", ConsoleColor.Cyan);

            Console.Write("Enter account number: ");
            string accNo = Console.ReadLine() ?? string.Empty;

            string pin = ReadPin("Enter PIN: ");

            var (success, message, account) = bank.Login(accNo, pin);

            if (!success || account is null)
            {
                WriteLine($"\n{message}", ConsoleColor.Red);
                return;
            }

            WriteLine($"\n{message}", ConsoleColor.Green);
            RunAccountSession(account);
        }

        // ---------- Handlers: logged-in session ----------

        private static void RunAccountSession(Account account)
        {
            bool loggedIn = true;
            while (loggedIn)
            {
                PrintAccountMenu(account);
                switch (ReadMenuChoice(1, 7))
                {
                    case 1:
                        WriteLine($"\nCurrent balance: {account.Balance:C}", ConsoleColor.Green);
                        break;
                    case 2:
                        {
                            decimal amount = ReadDecimal("Enter amount to deposit: ");
                            var (ok, msg) = bank.Deposit(account, amount);
                            WriteLine($"\n{msg}", ok ? ConsoleColor.Green : ConsoleColor.Red);
                            break;
                        }
                    case 3:
                        {
                            decimal amount = ReadDecimal("Enter amount to withdraw: ");
                            var (ok, msg) = bank.Withdraw(account, amount);
                            WriteLine($"\n{msg}", ok ? ConsoleColor.Green : ConsoleColor.Red);
                            break;
                        }
                    case 4:
                        {
                            Console.Write("Enter destination account number: ");
                            string toAcc = Console.ReadLine() ?? string.Empty;
                            decimal amount = ReadDecimal("Enter amount to transfer: ");
                            var (ok, msg) = bank.Transfer(account, toAcc, amount);
                            WriteLine($"\n{msg}", ok ? ConsoleColor.Green : ConsoleColor.Red);
                            break;
                        }
                    case 5:
                        {
                            var statement = bank.GetStatement(account);
                            WriteLine("\n-- Mini Statement --", ConsoleColor.Yellow);
                            if (statement.Count == 0)
                            {
                                Console.WriteLine("No transactions yet.");
                            }
                            else
                            {
                                foreach (var t in statement)
                                    Console.WriteLine(t);
                            }
                            break;
                        }
                    case 6:
                        {
                            string oldPin = ReadPin("Enter current PIN: ");
                            string newPin = ReadPin("Enter new 4-digit PIN: ");
                            var (ok, msg) = bank.ChangePin(account, oldPin, newPin);
                            WriteLine($"\n{msg}", ok ? ConsoleColor.Green : ConsoleColor.Red);
                            break;
                        }
                    case 7:
                        loggedIn = false;
                        WriteLine("\nLogged out.", ConsoleColor.Cyan);
                        break;
                }
            }
        }

        // ---------- Input helpers ----------

        private static int ReadMenuChoice(int min, int max)
        {
            while (true)
            {
                string input = Console.ReadLine() ?? string.Empty;
                if (int.TryParse(input, out int choice) && choice >= min && choice <= max)
                    return choice;

                Console.Write($"Invalid choice. Enter a number between {min} and {max}: ");
            }
        }

        private static string ReadPin(string prompt)
        {
            while (true)
            {
                Console.Write(prompt);
                string pin = Console.ReadLine() ?? string.Empty;
                if (pin.Length == 4 && pin.All(char.IsDigit))
                    return pin;

                Console.WriteLine("PIN must be exactly 4 digits. Try again.");
            }
        }

        private static decimal ReadDecimal(string prompt)
        {
            while (true)
            {
                Console.Write(prompt);
                string input = Console.ReadLine() ?? string.Empty;
                if (decimal.TryParse(input, out decimal value) && value >= 0)
                    return value;

                Console.WriteLine("Please enter a valid positive number.");
            }
        }

        private static void WriteLine(string text, ConsoleColor color)
        {
            Console.ForegroundColor = color;
            Console.WriteLine(text);
            Console.ResetColor();
        }
    }
}
