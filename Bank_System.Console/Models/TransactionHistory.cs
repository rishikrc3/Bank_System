public class TransactionHistory
{
    public decimal Amount {get; set;}
    public TransactionType TransactionType {get; set;}
    public DateTimeOffset Timestamp {get; set;}
    public decimal Balance {get; set;}
    public override string ToString()
    {
        return $"Type: {TransactionType} | Amount: {Amount:C} | Balance: {Balance:C} | Date: {Timestamp}";
    }
}