using Models;

public interface IAccountService
{
    Task<Guid> CreateUserAccountAsync(AccountCreatorDTO accountDTO, CancellationToken ct = default);
    Task<decimal> IncreaseAccountBalanceAsync(Guid id, string bal, CancellationToken ct = default);
    Task<decimal> DecreaseAccountBalanceAsync(Guid id, string bal, CancellationToken ct = default);
    Task<List<TransactionHistory>> GetTransactionHistoryAsync(Guid id, CancellationToken ct = default);
    Task<Account> GetAccountDetailsByIdAsync(Guid id, CancellationToken ct = default);
    Task<IEnumerable<Account>> GetAllAccountsAsync(CancellationToken ct = default);
    Task<FinancialModel> GetFinancialReportAsync(CancellationToken ct = default);
}