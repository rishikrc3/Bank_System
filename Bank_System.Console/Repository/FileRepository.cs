using System.Data.Common;
using System.Runtime.CompilerServices;
using System.Text.Json;
using Models;
public class FileRepository : IRepository
{

    public void AddUserAccount(Account account)
    {
        var filePath = "data.json";
        List<Account>accounts;
        var options = new JsonSerializerOptions { WriteIndented = true };
        if(File.Exists(filePath))
        {
            using FileStream fs = new FileStream(filePath, FileMode.Open, FileAccess.Read);
            if(fs.Length == 0)
            {
                accounts = new List<Account>();
            }
            else
            {
                accounts = JsonSerializer.Deserialize<List<Account>>(fs,options)?? new List<Account>();
            }
        }
        else
        {
            accounts = new List<Account>();
        }
        accounts.Add(account);
        using FileStream ws = new FileStream(filePath, FileMode.Create, FileAccess.Write);
        JsonSerializer.Serialize(ws, accounts,options);
    }
    public void DeleteUserAccount(Guid id)
    {
        throw new NotImplementedException();
    }

    public Account GetAccountDetailsById(Guid id)
    {
        throw new NotImplementedException();
    }

    public IEnumerable<Account> GetAllAccounts()
    {
        throw new NotImplementedException();
    }
}