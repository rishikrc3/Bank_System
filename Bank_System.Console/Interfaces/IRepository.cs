using System.ComponentModel;
using Models;

public interface IRepository
{
    void AddUserAccount(Account account);
    IEnumerable<Account> GetAllAccounts();
    Account GetAccountDetailsById(Guid id);
    void DeleteUserAccount(Guid id);
}