using System.ComponentModel;
using Models;

public interface IRepository<T> where T: class
{
    Guid AddUserAccount(T account);
    IEnumerable<T> GetAllAccounts();
    T GetAccountDetailsById(Guid id);
    decimal IncreaseAccountBalance(Guid id, decimal amount);
    decimal DecreaseAccountBalance(Guid id, decimal amount);
    List<TransactionHistory> GetTransactionHistory(Guid id);
    FinancialModel GetFinancialReport();
    void UpdateTransactionHistory(TransactionHistory transaction, T account);

}