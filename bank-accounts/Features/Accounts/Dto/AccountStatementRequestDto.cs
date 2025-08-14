namespace bank_accounts.Features.Accounts.Dto;

/// <summary>
/// Запрос на получение выписки по счету
/// </summary>
public class AccountStatementRequestDto
{
    /// <summary>
    /// Начальная дата периода выписки (включительно)
    /// </summary>
    /// <example>2025-03-01</example>
    public DateTime StartDate { get; set; }

    /// <summary>
    /// Конечная дата периода выписки (не включительно)
    /// </summary>
    /// <remarks>
    /// Должна быть больше StartDate.
    /// Максимальный период - 1 год.
    /// </remarks>
    /// <example>2025-03-31</example>
    public DateTime EndDate { get; set; }
}