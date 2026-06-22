using Delegates;
using Events;
using Exceptions;
using Models;
using System.Runtime.CompilerServices;
public class AccountService : IAccountService
{
    private readonly IRepository<Account> _repository;
    private readonly TransactionEvent _transactionEvent;
    private readonly LowBalanceEvent _lowBalanceEvent;
    private readonly Action<string> _logger;
    private readonly SemaphoreSlim _semaphore = new(1, 1);
    public AccountService(IRepository<Account>repository, TransactionEvent transactionEvent, LowBalanceEvent lowBalanceEvent, Action<string> logger)
    {
        _repository = repository;
        _transactionEvent = transactionEvent;
        _lowBalanceEvent = lowBalanceEvent;
        _logger = logger;
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
    public async Task<Guid> CreateUserAccountAsync(AccountCreatorDTO accountDTO, CancellationToken ct = default)
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
        await _repository.AddAsync(account, ct);
        _logger($"Log:Information, Account created with ID: {account.Id}");
        return account.Id;
    }
    public async Task<decimal> IncreaseAccountBalanceAsync(Guid id, string bal, CancellationToken ct = default)
    {
        decimal balance = ValidateAccountBalanceUpdateDetials(id,bal);
        var account = await _repository.GetByIdAsync(id);
        if (account == null) 
        {
            throw new AccountNotFoundException($"Account with ID {id} not found.", id);
        }
        await _semaphore.WaitAsync(ct);
        try
        {
            account.Balance += balance;
        }
        finally
        {
            _semaphore.Release();
        }
        TransactionHistory transaction = new TransactionHistory
        {
            Amount = balance,
            TransactionType = TransactionType.Deposit,
            Timestamp = DateTimeOffset.UtcNow,
            Balance = account.Balance
        };
        account.Transactions.Add(transaction);
        _transactionEvent.OnTransactionOccurred(account.Id, balance, TransactionType.Deposit, account.Balance);
        return account.Balance;
    }

    public async Task<decimal> DecreaseAccountBalanceAsync(Guid id, string bal, CancellationToken ct = default)
    { 
        decimal balance = ValidateAccountBalanceUpdateDetials(id,bal);
        var account = await _repository.GetByIdAsync(id);
        if (account == null)
        {   
            throw new AccountNotFoundException($"Account with ID {id} not found.", id);
        }
        if (account.Balance < balance)
        {
            throw new AccountBalanceException($"Insufficient funds. Shortfall: {balance - account.Balance}", account.Balance.ToString());
        }
        await _semaphore.WaitAsync(ct);
        try
        {
            account.Balance -= balance;
        }
        finally
        {
            _semaphore.Release();
        }
        TransactionHistory transaction = new TransactionHistory
        {
            Amount = balance,
            TransactionType = TransactionType.Withdraw,
            Timestamp = DateTimeOffset.UtcNow,
            Balance = account.Balance
        };
        account.Transactions.Add(transaction);
        _transactionEvent.OnTransactionOccurred(account.Id, balance, TransactionType.Withdraw, account.Balance);
        if(account.Balance < 100)
        {
            _lowBalanceEvent.OnLowBalanceOccured(account.Id, balance, TransactionType.Withdraw, account.Balance);
        }
        return account.Balance;
    }
    public async IAsyncEnumerable<TransactionHistory> GetTransactionHistoryAsync(Guid id, [EnumeratorCancellation] CancellationToken ct = default)
    {
        var account = await _repository.GetByIdAsync(id,ct);
        if (account == null)
        {
            throw new AccountNotFoundException($"Account with ID {id} not found.", id);
        }
        foreach (var transaction in account.Transactions)
        {
            await Task.Delay(1000);
            yield return transaction;
        }
    }
    public async Task<Account> GetAccountDetailsByIdAsync(Guid id, CancellationToken ct = default)
    {
        var account = await _repository.GetByIdAsync(id, ct);
        if (account == null)
        {
            throw new AccountNotFoundException($"Account with ID {id} not found.", id);
        }
        return account;
    }
    public async Task<IEnumerable<Account>> GetAllAccountsAsync(CancellationToken ct = default)
    {
        return await _repository.GetAllAsync(ct);
    }
    public async Task<FinancialModel> GetFinancialReportAsync(CancellationToken ct)
    {
        var accounts = await _repository.GetAllAsync(ct);
        var accountsList = accounts.ToList();
        if (accountsList == null || accountsList.Count == 0)
        {
            throw new NoAccountsException($"No accounts found.");
        }
        var totalAccoutns = accountsList.Count;
        var savingsAccounts = accountsList.Count(x => x.AccountType == AccountType.Savings);
        var currentAccounts = accountsList.Count(x => x.AccountType == AccountType.Current);
        var totalBalance = accountsList.Sum(x => x.Balance);
        var highestBalanceId = accountsList.MaxBy(x => x.Balance).Id;
        var lowestBalanceId = accountsList.MinBy(x => x.Balance).Id;

        var totalTansactions = accountsList.Sum(x => x.Transactions.Count);
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
    public void RunAccountAction(AccountAction accountAction)
    {
        var accounts = _repository.GetAll();
        foreach (var account in accounts)
        {
            accountAction(account);
        }
    }
}