using Models;

public interface IAccountService
{
    Guid CreateUserAccount(AccountCreatorDTO accountDTO);
    decimal IncreaseAccountBalance(Guid id, string bal);
    decimal DecreaseAccountBalance(Guid id, string bal);
    List<TransactionHistory> GetTransactionHistory(Guid id);
    Account GetAccountDetailsById(Guid id);
    IEnumerable<Account> GetAllAccounts();
    FinancialModel GetFinancialReport();
}