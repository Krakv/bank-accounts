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
public class MbResult<T>
{
    /// <summary>
    /// Конструктор для успешного результата
    /// </summary>
    [JsonConstructor]
    public MbResult(string title, int statusCode, T? value, IReadOnlyDictionary<string, string>? errors = null)
    {
        Title = title;
        StatusCode = statusCode;
        Value = value;
        Errors = errors;
    }

    /// <summary>
    /// Конструктор для результата с ошибками
    /// </summary>
    public MbResult(string title, int statusCode, IReadOnlyDictionary<string, string> errors)
        : this(title, statusCode, default, errors)
    {
    }

    /// <summary>
    /// Заголовок, описывающий результат операции.
    /// </summary>
    /// <example>Account created successfully</example>
    public string Title { get; }

    /// <summary>
    /// HTTP-статус код результата.
    /// </summary>
    /// <example>201</example>
    public int StatusCode { get; }

    /// <summary>
    /// Основное возвращаемое значение операции.
    /// </summary>
    /// <example>3fa85f64-5717-4562-b3fc-2c963f66afa6</example>
    public T? Value { get; }

    /// <summary>
    /// Словарь ошибок, если операция завершилась с ошибками.
    /// </summary>
    /// <example>
    /// {
    ///     "OwnerId": "The OwnerId field is required.",
    ///     "Type": "Invalid account type."
    /// }
    /// </example>
    public IReadOnlyDictionary<string, string>? Errors { get; }
}