namespace bank_accounts.Exceptions;

public class ValidationAppException(IReadOnlyDictionary<string, string> errors) : Exception("One or more validation errors occured")
{
    public IReadOnlyDictionary<string, string> Errors { get; } = errors;
}