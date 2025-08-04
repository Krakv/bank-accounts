using System.Text.Json.Serialization;

namespace bank_accounts.Features.Common;

/// <summary>
/// Результат выполнения операции, содержащий статус, данные и возможные ошибки.
/// </summary>
/// <typeparam name="T">Тип возвращаемого значения.</typeparam>
/// <remarks>
/// Пример успешного ответа:
/// 
///     {
///         "title": "Success",
///         "statusCode": 201,
///         "value": "3fa85f64-5717-4562-b3fc-2c963f66afa6"
///     }
///     
/// Пример ответа с ошибками:
/// 
///     {
///         "title": "Validation errors occurred",
///         "statusCode": 400,
///         "errors":
///         {
///             "OwnerId": "The OwnerId field is required.",
///             "Type": "Invalid account type.",
///         }
///     }
/// </remarks>
public class MbResult<T>(string title, int statusCode, T? value)
{
    /// <summary>
    /// Заголовок, описывающий результат операции.
    /// </summary>
    /// <example>Account created successfully</example>
    [JsonInclude]
    public string Title { get; } = title;

    /// <summary>
    /// HTTP-статус код результата.
    /// </summary>
    /// <example>201</example>
    [JsonInclude]
    public int StatusCode { get; } = statusCode;

    /// <summary>
    /// Основное возвращаемое значение операции.
    /// </summary>
    /// <example>3fa85f64-5717-4562-b3fc-2c963f66afa6</example>
    [JsonInclude]
    public T? Value { get; } = value;

    /// <summary>
    /// Словарь ошибок, если операция завершилась с ошибками.
    /// </summary>
    /// <example>
    /// {
    ///     "OwnerId": "The OwnerId field is required.",
    ///     "Type": "Invalid account type."
    /// }
    /// </example>
    [JsonInclude]
    public IReadOnlyDictionary<string, string>? Errors { get; }

    /// <summary>
    /// Конструктор для результата с ошибками.
    /// </summary>
    /// <param name="title">Заголовок результата.</param>
    /// <example>Validation errors occurred</example>
    /// <param name="statusCode">HTTP-статус код.</param>
    /// <example>400</example>
    /// <param name="errors">Словарь ошибок (ключ — название ошибки, значение — описание).</param>
    /// <example>
    /// {
    ///     "OwnerId": "The OwnerId field is required.",
    ///     "Type": "Invalid account type."
    /// }
    /// </example>
    public MbResult(string title, int statusCode, IReadOnlyDictionary<string, string> errors)
        : this(title, statusCode, default(T?))
    {
        Errors = errors;
    }
}