using Exceptions;
using Models;
public class AccountService : IAccountService
{
    private readonly IRepository<Account> _repository;
    public AccountService(IRepository<Account>repository)
    {
        _repository = repository;
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
        _repository.Add(account);
        return account.Id;
    }
    public decimal IncreaseAccountBalance(Guid id, string bal)
    {
        decimal balance = ValidateAccountBalanceUpdateDetials(id,bal);
        var account = _repository.GetById(id);
        if (account == null) 
        {
            throw new ArgumentException($"Account with ID {id} not found.", nameof(id));
        }
        account.Balance += balance;
        TransactionHistory transaction = new TransactionHistory
        {
            Amount = balance,
            TransactionType = TransactionType.Deposit,
            Timestamp = DateTimeOffset.UtcNow,
            Balance = account.Balance
        };
        account.Transactions.Add(transaction);
        return account.Balance;
    }

    public decimal DecreaseAccountBalance(Guid id, string bal)
    { 
        decimal balance = ValidateAccountBalanceUpdateDetials(id,bal);
        var account = _repository.GetById(id);
        if (account == null)
        {
            throw new ArgumentException($"Account with ID {id} not found.", nameof(id));
        }
        if (account.Balance < balance)
        {
            throw new AccountBalanceException($"Insufficient funds. Shortfall: {balance - account.Balance}", account.Balance.ToString());
        }
        account.Balance -= balance;
        TransactionHistory transaction = new TransactionHistory
        {
            Amount = balance,
            TransactionType = TransactionType.Withdraw,
            Timestamp = DateTimeOffset.UtcNow,
            Balance = account.Balance
        };
        account.Transactions.Add(transaction);
        return account.Balance;
    }

    public List<TransactionHistory> GetTransactionHistory(Guid id)
    {
        var account  = _repository.GetById(id);
        if (account == null)
        {
            throw new ArgumentException($"Account with ID {id} not found.", nameof(id));
        }
        return account.Transactions;
    }

    public Account GetAccountDetailsById(Guid id)
    {
        var account = _repository.GetById(id);
        if (account == null)
        {
            throw new ArgumentException($"Account with ID {id} not found.", nameof(id));
        }
        return account;
    }
    public IEnumerable<Account> GetAllAccounts()
    {
        return _repository.GetAll();
    }

    public FinancialModel GetFinancialReport()
    {
        var accounts = _repository.GetAll().ToList();
        if(accounts == null || accounts.Count == 0)
        {
            throw new ArgumentException($"No accounts found.");
        }
        var totalAccoutns = accounts.Count;
        var savingsAccounts = accounts.Count(x => x.AccountType == AccountType.Savings);
        var currentAccounts = accounts.Count(x => x.AccountType == AccountType.Current);
        var totalBalance = accounts.Sum(x => x.Balance);
        var highestBalanceId = accounts.MaxBy(x => x.Balance).Id;
        var lowestBalanceId = accounts.MinBy(x => x.Balance).Id;

        var totalTansactions = accounts.Sum(x => x.Transactions.Count);
        return new FinancialModel
        {
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