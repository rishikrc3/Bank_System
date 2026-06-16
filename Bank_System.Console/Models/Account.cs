namespace Models
{
    public class Account
    {
        public Guid Id {get; init;}
        public string HolderName {get; set;} = string.Empty;
        public decimal Balance  {get; set;} = 0;
        public AccountType AccountType {get;set;}
        public DateTimeOffset DateCreated { get;  set;}
        public List<TransactionHistory> Transactions { get; set; } = new List<TransactionHistory>();
    }
}
