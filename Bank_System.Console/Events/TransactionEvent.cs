using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Events
{
    public class TransactionEvent
    {
        public event EventHandler<TransactionEventArgs> TransactionOccurred;
        public void OnTransactionOccurred(Guid accountId, decimal amount, TransactionType transactionType, decimal totalBalance)
        {
            Console.WriteLine("Transaction Occured");
            TransactionOccurred?.Invoke(this, new TransactionEventArgs
            {
                AccountId = accountId,
                Amount = amount,
                TransactionType = transactionType,
                TotalBalance = totalBalance
            });
        }
    }
}
