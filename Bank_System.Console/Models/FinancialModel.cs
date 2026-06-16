using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models
{
    public class FinancialModel
    {
        public int TotalAccounts { get; set; }
        public decimal TotalBalance { get; set; }
        public int CurrentAccounts { get; set; }
        public int SavingsAccounts { get; set; }
        public Guid HighestBalanceId { get; set; }
        public Guid LowestBalanceId { get; set; }
        public int TotalTransactions { get; set; }
    }
}
