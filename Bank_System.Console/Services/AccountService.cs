using Exceptions;
using Models;
public class AccountService
{
    List<Account> accounts = new List<Account>();
    private Account FindAccount(Guid id)
    {
        var account = accounts.FirstOrDefault(x=>x.Id==id);
        if(account==null)
        {
            throw new AccountNotFoundException("Account does not exist", id);
        }
        return account;
    }

    private void UpdateTransactionHistory(TransactionHistory transaction, Account account)
    {
        account.Transactions.Add(transaction);
    }
    private decimal ValidateAccountCreationInput(AccountCreatorDTO accountCreatorDTO)
    {
        if(string.IsNullOrWhiteSpace(accountCreatorDTO.HolderName))
        {
            throw new ArgumentNullException("Holder name cannot be empty or whitespace", nameof(accountCreatorDTO.HolderName));
        }
        decimal balance;
        if (string.IsNullOrWhiteSpace(accountCreatorDTO.Balance) || !decimal.TryParse(accountCreatorDTO.Balance, out balance))
        {
            throw new AccountBalanceException("Invalid Balance.", accountCreatorDTO.Balance);
        }
        else if (balance <= 0.0M)
        {
            throw new AccountBalanceException("Insufficient balance.", accountCreatorDTO.Balance);  
        }
        return balance;
    }
    private decimal ValidateAccountBalanceUpdateDetials(Guid id, String bal)
    {
        if(id == Guid.Empty)
        {
            throw new ArgumentNullException("Id cannot be null", nameof(id));
        }
        decimal balance;
        if (string.IsNullOrWhiteSpace(bal) || !decimal.TryParse(bal, out balance))
        {
            throw new AccountBalanceException("Invalid Balance.", bal);
        }
        return balance;
    }
    public Guid CreateUserAccount(AccountCreatorDTO accountDTO)
    {
        decimal balance = ValidateAccountCreationInput(accountDTO);
        Account account = new Account
        {
            Id = Guid.NewGuid(),
            HolderName = accountDTO.HolderName,
            Balance = balance,
            AccountType = accountDTO.AccountType,
            DateCreated = accountDTO.DateCreated 
        };
        accounts.Add(account);
        return account.Id;
    }
    public decimal IncreaseAccountBalance(Guid id, string bal)
    {
        decimal balance = ValidateAccountBalanceUpdateDetials(id,bal);
        var account = FindAccount(id);
        account.Balance+=balance;
        TransactionHistory transaction = new TransactionHistory
        {
            Amount = balance,
            TransactionType = TransactionType.Deposit,
            Timestamp = DateTimeOffset.UtcNow,
            Balance = account.Balance
        };
        UpdateTransactionHistory(transaction, account);
        return account.Balance;
    }

    public decimal DecreaseAccountBalance(Guid id, string bal)
    { 
        decimal balance = ValidateAccountBalanceUpdateDetials(id,bal);
        var account = FindAccount(id);
        //exception handler here
        if(balance > account.Balance)
            throw new AccountBalanceException($"Insufficient funds. Shortfall: {balance - account.Balance}", bal);
        account.Balance-=balance;
        TransactionHistory transaction = new TransactionHistory
        {
            Amount = balance,
            TransactionType = TransactionType.Withdraw,
            Timestamp = DateTimeOffset.UtcNow,
            Balance = account.Balance
        };
        UpdateTransactionHistory(transaction, account);
        return account.Balance;
    }

    public List<TransactionHistory> GetTransactionHistory(Guid id)
    {
        var account = FindAccount(id);
        if(!account.Transactions.Any())
            throw new NotransactionsException("No transactions found for this account", id);
        return account.Transactions;
    }

    public Account GetAccountDetailsById(Guid id)
    {
        var account = FindAccount(id);
        return account;
    }

    public IEnumerable<Account> GetAllAccounts()
    {
        return accounts.OrderByDescending(x => x.Balance);
    }

    public FinancialModel GetFinancialReport()
    {
        if (accounts == null)
        {
            throw new NoAccountsException("No accounts exist in the system.");
        }
        var totalAccoutns = accounts.Count;
        var savingsAccounts = accounts.Count(x => x.AccountType == AccountType.Savings);
        var currentAccounts = accounts.Count(x => x.AccountType == AccountType.Current);
        var totalBalance = accounts.Sum(x => x.Balance);
        var highestBalanceId = accounts.MaxBy(x => x.Balance).Id;
        var lowestBalanceId = accounts.MinBy(x => x.Balance).Id;

        var totalTansactions = accounts.Sum(x => x.Transactions.Count);
        return new FinancialModel { 
            TotalAccounts = totalAccoutns,
            SavingsAccounts = savingsAccounts,
            CurrentAccounts = currentAccounts,
            TotalBalance = totalBalance,
            HighestBalanceId = highestBalanceId,
            LowestBalanceId = lowestBalanceId,
            TotalTransactions = totalTansactions
        };
    }
}