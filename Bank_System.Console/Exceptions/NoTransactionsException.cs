class NotransactionsException : Exception
{
    public Guid Id {get;}
    public NotransactionsException(string message, Guid accountId) : base(message)
    {
        Id = accountId;
    }
}
