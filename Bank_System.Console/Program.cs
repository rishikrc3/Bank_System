using Exceptions;
using Extensions;
using Models;
using Repository;
using System.Net.Http.Json;
using System.Text;
class Bank_System
{
    public async Task GetExchangeRateAsync()
    {
        const string url = $"https://open.er-api.com/v6/latest/GBP";
        try
        {
            using (var client = new HttpClient())
            {
                GBPResponse response = await client.GetFromJsonAsync<GBPResponse>(url);
                if(response == null)
                {
                    Console.WriteLine("Failed to retrieve exchange rate.");
                    return;
                }
                if (response.Result=="success")
                {
                    Console.WriteLine($"Exchange rate for GBP to USD: {response.Rates["USD"]}");
                }
                else
                {
                    Console.WriteLine("Failed to retrieve exchange rate.");
                }
            }
        }
        catch (HttpRequestException ex)
        {
            Console.WriteLine($"Request error: {ex.Message}");
        }
        catch (TaskCanceledException)
        {
            Console.WriteLine("Request timed out. Please try again later.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"An unexpected error occurred: {ex.Message}");
        }
    }
    public static async Task Main(String []args)
    {
        var tokenSource = new CancellationTokenSource();
        string path = Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "accounts.json");
        IRepository<Account> repository = new FileRepository<Account>(path);
        IAccountService accountService = new AccountService(repository);
        Bank_System bank_System = new Bank_System();
        await Task.WhenAll(bank_System.GetExchangeRateAsync(), bank_System.ViewAllAccountsAsync(accountService, tokenSource.Token));
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
            Console.CancelKeyPress += (sender, eventArgs) =>
            {
                Console.WriteLine("Cancelling...");
                tokenSource.Cancel();
                eventArgs.Cancel = true;
            };
            if (num == 8) 
                break;
            switch(num)
            {
                case 1:
                    await bank_System.CreateUserAccountAsync(accountService, tokenSource.Token);
                    break;   
                case 2:
                    await bank_System.DepositMoneyAsync(accountService, tokenSource.Token);
                    break;

                case 3:
                    await bank_System.WithdrawMoneyAsync(accountService, tokenSource.Token);
                    break;    
                case 4:
                    await bank_System.ViewTransactionsAsync(accountService, tokenSource.Token);
                    break;
                case 5:
                    await bank_System.ViewAccountDetailsByIdAsync(accountService, tokenSource.Token);
                    break;
                case 6:
                    await bank_System.ViewAllAccountsAsync(accountService, tokenSource.Token);
                    break;
                case 7:
                    await bank_System.ViewFinancialModelAsync(accountService, tokenSource.Token);
                    break;
            }
        }
    }
    private async Task CreateUserAccountAsync(IAccountService accountService, CancellationToken cancellationToken)
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
            var id = await accountService.CreateUserAccountAsync(accountDTO, cancellationToken);
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
    private async Task DepositMoneyAsync(IAccountService accountService, CancellationToken cancellationToken)
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
            var updatedBalance = await accountService.IncreaseAccountBalanceAsync(parsedId, updateBalance, cancellationToken);
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
    private async Task WithdrawMoneyAsync(IAccountService accountService, CancellationToken cancellationToken)
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
            var balanceAfterUpdating  = await accountService.DecreaseAccountBalanceAsync(parsedId, updateBalance, cancellationToken);
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
    public async Task ViewTransactionsAsync(IAccountService accountService, CancellationToken cancellationToken)
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
            await foreach (var transaction in accountService.GetTransactionHistoryAsync(parsedId, cancellationToken))
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
    public async Task ViewAccountDetailsByIdAsync(IAccountService accountService, CancellationToken cancellationToken)
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
            await foreach(var transaction in accountService.GetTransactionHistoryAsync(parsedId, cancellationToken))
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
    public async Task ViewAllAccountsAsync(IAccountService accountService, CancellationToken cancellationToken)
    {
        try
        { 
            var accounts = await accountService.GetAllAccountsAsync(cancellationToken);
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
    public async Task ViewFinancialModelAsync(IAccountService accountService, CancellationToken cancellationToken)
    {
        try
        {
            var accountsData = await accountService.GetFinancialReportAsync(cancellationToken);
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
