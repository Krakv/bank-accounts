namespace bank_accounts.Exceptions;

public class FrozenAccountException(Guid accountId) : Exception($"Account {accountId} is frozen, transactions are not allowed");