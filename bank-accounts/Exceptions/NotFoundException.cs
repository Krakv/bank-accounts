namespace bank_accounts.Exceptions;

public class NotFoundAppException : Exception
{
    public NotFoundAppException(string message) : base(message)
    {
    }

    public NotFoundAppException(string entityName, Guid id)
        : base($"{entityName} with id {id} was not found")
    {
        EntityName = entityName;
        Id = id;
    }

    public string? EntityName { get; }
    public Guid Id { get; }
}