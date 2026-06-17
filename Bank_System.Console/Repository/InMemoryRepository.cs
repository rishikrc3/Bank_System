using Exceptions;
using Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repository
{
    public class InMemoryAccountRepository : IRepository<Account>
    {
        List<Account> accounts = new List<Account>();
        public Guid AddUserAccount(Account account)
        {
            accounts.Add(account);
            return account.Id;
        }
        public Account GetAccountDetailsById(Guid id)
        {
            var account = accounts.FirstOrDefault(x=>x.Id == id);
            if (account == null)
            {
                throw new AccountNotFoundException("Account does not exist", id);
            }
            return account;
        }
        public decimal IncreaseAccountBalance(Guid id, decimal amount)
        {
            var account = GetAccountDetailsById(id);
            account.Balance += amount;
            TransactionHistory transaction = new TransactionHistory
            {
                Amount = amount,
                TransactionType = TransactionType.Deposit,
                Timestamp = DateTimeOffset.UtcNow,
                Balance = account.Balance
            };
            UpdateTransactionHistory(transaction, account);
            return account.Balance;
        }

        public decimal DecreaseAccountBalance(Guid id, decimal amount)
        {
            var account = GetAccountDetailsById(id);
            if (account.Balance < amount)
            {
                throw new AccountBalanceException($"Insufficient funds. Shortfall: {amount - account.Balance}", account.Balance.ToString());
            }
            account.Balance -= amount;
            TransactionHistory transaction = new TransactionHistory
            {
                Amount = amount,
                TransactionType = TransactionType.Withdraw,
                Timestamp = DateTimeOffset.UtcNow,
                Balance = account.Balance
            };
            UpdateTransactionHistory(transaction, account);
            return account.Balance;
        }
        public IEnumerable<Account> GetAllAccounts()
        {
           return accounts.OrderByDescending(x => x.Balance);
        }

        public List<TransactionHistory> GetTransactionHistory(Guid id)
        {
            var account = GetAccountDetailsById(id);
            if (!account.Transactions.Any())
                throw new NotransactionsException("No transactions found for this account", id);
            return account.Transactions;
        }

        public FinancialModel GetFinancialReport()
        {
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
        private void UpdateTransactionHistory(TransactionHistory transaction, Account account)
        {
            account.Transactions.Add(transaction);
        }
    }
}
