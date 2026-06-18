using Exceptions;
using Extensions;
using Models;
using Repository;
using System.Text;
class Bank_System
{
    public async Task<string> GetExchangeRateAsync()
    {
        var url = $"https://open.er-api.com/v6/latest/GBP";
        try
        {
            using (var client = new HttpClient())
            {
                var response = await client.GetAsync(url);
                if(response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    return json;
                }
                return $"Failed to fetch weather. Status: {response.StatusCode}";
            }
        }
        catch (HttpRequestException ex)
        {
            return $"Request error: {ex.Message}";
        }
        catch (TaskCanceledException)
        {
            return "Request timed out. Please try again later.";
        }
        catch (Exception ex)
        {
            return $"An unexpected error occurred: {ex.Message}";
        }
    }
    public static async Task Main(String []args)
    {
        string path = Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "accounts.json");
        IRepository<Account> repository = new InMemoryRepository<Account>();
        IAccountService accountService = new AccountService(repository);
        Bank_System bank_System = new Bank_System();
        var json = await bank_System.GetExchangeRateAsync();
        Console.WriteLine($"Exchange Rate Data: {json}");
        while (true)
        {
            Console.WriteLine("1. Create account");
            Console.WriteLine("2. Deposit");
            Console.WriteLine("3. Withdraw");
            Console.WriteLine("4.  View Transaction History");
            Console.WriteLine("5.  View Account Details");
            Console.WriteLine("6. View All Accounts");
            Console.WriteLine("7. View Fianancial Model");
            Console.WriteLine("8. Exit");
            
            if(!int.TryParse(Console.ReadLine(), out int num))
            {
                Console.WriteLine("Invalid option, try again");
                continue;
            }
            if(num == 8) 
                break;
            switch(num)
            {
                case 1:
                    bank_System.CreateUserAccount(accountService);
                    break;   

                case 2:
                    bank_System.DepositMoney(accountService);
                    break;

                case 3:
                    bank_System.WithdrawMoney(accountService);
                    break;    
                case 4:
                    bank_System.ViewTransactions(accountService);
                    break;
                case 5:
                    bank_System.ViewAccountDetailsById(accountService);
                    break;
                case 6:
                    bank_System.ViewAllAccounts(accountService);
                    break;
                case 7:
                    bank_System.ViewFinancialModel(accountService);
                    break;
            }
        }
    }
    private async Task CreateUserAccount(IAccountService accountService)
    {
        try
        {
            Console.WriteLine("Enter holder name:");
            string holderName = Console.ReadLine();

            Console.WriteLine("Enter opening balance:");
            string openingBalance = Console.ReadLine();

            Console.WriteLine("Select account type: 1 = Current, 2 = Savings");
            string typeInput = Console.ReadLine();

            AccountType accountType = typeInput switch
            {
                "1" => AccountType.Current,
                "2" => AccountType.Savings,
                _ => throw new Exception("Invalid account type")
            };
            AccountCreatorDTO accountDTO = new AccountCreatorDTO
            {
                HolderName = holderName,
                Balance  = openingBalance,
                AccountType = accountType
            };
            var id = await accountService.CreateUserAccountAsync(accountDTO);
            Console.WriteLine($"Account created successfully. ID: {id}");        
        }
        catch(ArgumentNullException ex)
        {
            Console.WriteLine(ex.Message);
        }  
        catch(AccountBalanceException ex)
        {
            Console.WriteLine(ex.Message);
        }
        catch(Exception ex)
        {
            Console.WriteLine($"Exception occured: {ex.Message}");
        }
    }
    private async Task DepositMoney(IAccountService accountService)
    {
        try
        {
            Console.WriteLine("Enter account ID:");
            string accountId = Console.ReadLine();

            Console.WriteLine("Enter money to be deposited");
            string updateBalance = Console.ReadLine();
            
            if(!Guid.TryParse(accountId, out Guid parsedId))
            {
                Console.WriteLine("Invalid account ID format");
                return;
            }
            var updatedBalance = await accountService.IncreaseAccountBalanceAsync(parsedId, updateBalance);
            Console.WriteLine($"Balance after updating: {updatedBalance}");
        }
        catch(AccountNotFoundException ex)
        {
            Console.WriteLine(ex.Message);
        }
        catch(AccountBalanceException ex)
        {
            Console.WriteLine(ex.Message);
        }
        catch(Exception ex)
        {
            Console.WriteLine($"Exception occured: {ex.Message}");
        }
    }
    private async Task WithdrawMoney(IAccountService accountService)
    {
        try
        {
            Console.WriteLine("Enter account ID:");
            string accountId = Console.ReadLine();

            Console.WriteLine("Enter money to be withdrawn");
            string updateBalance = Console.ReadLine();
            if(!Guid.TryParse(accountId, out Guid parsedId))
            {
                Console.WriteLine("Invalid account ID format");
                return;
            }
            var balanceAfterUpdating  = await accountService.DecreaseAccountBalanceAsync(parsedId, updateBalance);
            Console.WriteLine($"Balance after updating: {balanceAfterUpdating}");
        }
        catch(AccountNotFoundException ex)
        {
            Console.WriteLine(ex.Message);
        }
        catch(AccountBalanceException ex)
        {
            Console.WriteLine(ex.Message);
        }
        catch(Exception ex)
        {
            Console.WriteLine($"Exception occured: {ex.Message}");
        }
    }
    public async Task ViewTransactions(IAccountService accountService)
    {
        try
        {
            Console.WriteLine("Enter account ID:");
            string accountId = Console.ReadLine();
            if(!Guid.TryParse(accountId, out Guid parsedId))
            {
                Console.WriteLine("Invalid account ID format");
                return;
            }
            List<TransactionHistory> trasactions =await  accountService.GetTransactionHistoryAsync(parsedId);
            foreach(var transaction in trasactions)
            {
                Console.WriteLine(transaction);
            }
        }
        catch(AccountNotFoundException ex)
        {
            Console.WriteLine(ex.Message);
        }
        catch(NotransactionsException ex)
        {
            Console.WriteLine(ex.Message);
        }
        catch(Exception ex)
        {
            Console.WriteLine($"Exception occured: {ex.Message}");
        }
    }
    public async Task ViewAccountDetailsById(IAccountService accountService)
    {
        try 
        {
            Console.WriteLine("Enter account ID:");
            string accountId = Console.ReadLine();
            if (!Guid.TryParse(accountId, out Guid parsedId))
            {
                Console.WriteLine("Invalid account ID format");
                return;
            }
            var account = await accountService.GetAccountDetailsByIdAsync(parsedId);
            Console.WriteLine("Account Details:");
            Console.WriteLine("Account Holder Name: " + account.HolderName);
            Console.WriteLine("Account Balance: " + account.Balance);
            Console.WriteLine("Account Type: " + account.AccountType);
            Console.WriteLine("Date Created: " + account.DateCreated);
            Console.WriteLine("Amount Deposited: " + account.Transactions.GetTotalDeposits());
            Console.WriteLine("Amount Withdrawn: " + account.Transactions.GetTotalWithdrawals());
            Console.WriteLine("Total Transactions: " + account.Transactions.Count);
            Console.WriteLine("Transaction History:");
            List<TransactionHistory> trasactions = await accountService.GetTransactionHistoryAsync(parsedId);
            foreach (var transaction in trasactions)
            {
                Console.WriteLine(transaction);
            }
        }
        catch (AccountNotFoundException ex)
        {
            Console.WriteLine(ex.Message);
        }
        catch (NotransactionsException ex)
        {
            Console.WriteLine(ex.Message);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Exception occured: {ex.Message}");
        }
    }
    public async Task ViewAllAccounts(IAccountService accountService)
    {
        try
        { 
            var accounts = await accountService.GetAllAccountsAsync();
            if (accounts == null)
            {
                Console.WriteLine("There are no accounts available.");
                return;
            }
            Console.WriteLine("All Accounts:");
            foreach (var account in accounts)
            {
                Console.WriteLine($"Account ID: {account.Id}, Holder Name: {account.HolderName}, Balance: {account.Balance}, Account Type: {account.AccountType}");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Exception occurred: {ex.Message}");
        }
    }
    public async Task ViewFinancialModel(IAccountService accountService)
    {
        try
        {
            var accountsData = await accountService.GetFinancialReportAsync();
            Console.WriteLine("Financial Report:");
            var sb = new StringBuilder();
            sb.AppendLine($"Total accounts: {accountsData.TotalAccounts}");
            sb.AppendLine($"Savings accounts: {accountsData.SavingsAccounts}");
            sb.AppendLine($"Current accounts: {accountsData.CurrentAccounts}");
            sb.AppendLine($"Total balance: {accountsData.TotalBalance}");
            sb.AppendLine($"Highest balance account: {accountsData.HighestBalanceId}");
            sb.AppendLine($"Lowest balance account: {accountsData.LowestBalanceId}");
            sb.AppendLine($"Total transactions: {accountsData.TotalTransactions}");
            Console.WriteLine(sb.ToString());
        }
        catch(NoAccountsException ex)
        {
            Console.WriteLine(ex.Message);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Exception occured: {ex.Message}");
        }
    }
}
