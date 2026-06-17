using System.ComponentModel;
using Models;

public interface IRepository<T> where T: class
{
    void AddUserAccount(T account);
    IEnumerable<T> GetAllAccounts();
    T GetAccountDetailsById(Guid id);
   
}