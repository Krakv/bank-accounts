namespace bank_accounts.Features.Transactions
{
    public class AccountNotFoundException : Exception
    {
        public AccountNotFoundException(Guid accountId)
            : base($"Account with id {accountId} not found") { }
    }

    public class InsufficientFundsException : Exception
    {
        public InsufficientFundsException(Guid accountId)
            : base($"Account with id {accountId} has insufficient funds") { }
    }

    public class CurrencyMismatchException : Exception
    {
        public CurrencyMismatchException(string accountCurrency, string counterpartyCurrency)
            : base($"Currency mismatch: account has {accountCurrency}, counterparty has {counterpartyCurrency}")
        {
        }
    }
}
