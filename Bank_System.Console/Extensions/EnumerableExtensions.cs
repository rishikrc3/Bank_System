using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bank_System.Console.Extensions
{
    public static class EnumerableExtensions
    {
        public static decimal GetTotalDeposits(this IEnumerable<TransactionHistory> transactions)
        {
            return transactions.Where(t => t.TransactionType == TransactionType.Deposit).Sum(t => t.Amount);
        }
        public static decimal GetTotalWithdrawals(this IEnumerable<TransactionHistory> transactions)
        {
            return transactions.Where(t => t.TransactionType == TransactionType.Withdraw).Sum(t => t.Amount);
        }
    }
}
