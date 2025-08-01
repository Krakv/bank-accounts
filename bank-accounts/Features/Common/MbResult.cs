using System.Text.Json.Serialization;

namespace bank_accounts.Features.Common;

public class MbResult<T>(string title, int statusCode, T? value)
{
    [JsonInclude] public string Title { get; } = title;
    [JsonInclude] public int StatusCode { get; } = statusCode;
    [JsonInclude] public T? Value { get; } = value;
    [JsonInclude] public IReadOnlyDictionary<string, string>? Errors { get; }

    public MbResult(string title, int statusCode, IReadOnlyDictionary<string, string> errors) : this(title, statusCode, default(T?))
    {
        Errors = errors;
    }
}