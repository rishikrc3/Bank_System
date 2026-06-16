using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using Bank_System.Console.Extensions;
using Models;


class Bank_System
{
    public static void Main(String []args)
    {
        AccountService accountService = new AccountService();
        Bank_System bank_System = new Bank_System();
        while(true)
        {
            Console.WriteLine("1. Create account");
            Console.WriteLine("2. Deposit");
            Console.WriteLine("3. Withdraw");
            Console.WriteLine("4.  View Transaction History");
            Console.WriteLine("5.  View Account Details");
            Console.WriteLine("6. View All Accounts");
            Console.WriteLine("7. Exit");
            
            if(!int.TryParse(Console.ReadLine(), out int num))
            {
                Console.WriteLine("Invalid option, try again");
                continue;
            }
            if(num == 7) 
                break;
            switch(num)
            {
                case 1:
                    bank_System.CreateUserAccount(accountService);
                    break;   

                case 2:
                    bank_System.DepositMoney(accountService);
                    break;

                case 3:
                    bank_System.WithdrawMoney(accountService);
                    break;    
                case 4:
                    bank_System.ViewTransactions(accountService);
                    break;
                case 5:
                    bank_System.ViewAccountDetailsById(accountService);
                    break;
                case 6:
                    bank_System.ViewAllAccounts(accountService);
                    break;
                    
            }
        }
    }
    private void CreateUserAccount(AccountService accountService)
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
            var id = accountService.CreateUserAccount(accountDTO);
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

    private void DepositMoney(AccountService accountService)
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
            var updatedBalance = accountService.IncreaseAccountBalance(parsedId, updateBalance);
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

    private void WithdrawMoney(AccountService accountService)
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
            var balanceAfterUpdating  = accountService.DecreaseAccountBalance(parsedId, updateBalance);
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

    public void ViewTransactions(AccountService accountService)
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
            List<TransactionHistory> trasactions = accountService.GetTransactionHistory(parsedId);
            foreach(var transaction in trasactions)
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

    public void ViewAccountDetailsById(AccountService accountService)
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
            var account = accountService.GetAccountDetailsById(parsedId);
            Console.WriteLine("Account Details:");
            Console.WriteLine("Account Holder Name: " + account.HolderName);
            Console.WriteLine("Account Balance: " + account.Balance);
            Console.WriteLine("Account Type: " + account.AccountType);
            Console.WriteLine("Date Created: " + account.DateCreated);
            Console.WriteLine("Amount Deposited: " + account.Transactions.GetTotalDeposits());
            Console.WriteLine("Amount Withdrawn: " + account.Transactions.GetTotalWithdrawals());
            Console.WriteLine("Total Transactions: " + account.Transactions.Count);
            Console.WriteLine("Transaction History:");
            List<TransactionHistory> trasactions = accountService.GetTransactionHistory(parsedId);
            foreach (var transaction in trasactions)
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
    public void ViewAllAccounts(AccountService accountService)
    {
        try
        { 
            var accounts = accountService.GetAllAccounts();
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
}
