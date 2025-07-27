namespace bank_accounts.Features.Accounts.Dto;

/// <summary>
/// Ответ с выпиской по счету
/// </summary>
public class AccountStatementResponseDto
{
    /// <summary>Идентификатор счета</summary>
    /// <example>3fa85f64-5717-4562-b3fc-2c963f66afa6</example>
    public Guid AccountId { get; set; }

    /// <summary>Идентификатор владельца счета</summary>
    /// <example>3fa85f64-5717-4562-b3fc-2c963f66afa6</example>
    public Guid OwnerId { get; set; }

    /// <summary>Валюта счета (ISO код)</summary>
    /// <example>EUR</example>
    public string Currency { get; set; } = string.Empty;

    /// <summary>Начальная дата периода выписки</summary>
    /// <example>2025-03-01</example>
    public DateTime StartDate { get; set; }

    /// <summary>Конечная дата периода выписки</summary>
    /// <example>2025-03-31</example>
    public DateTime EndDate { get; set; }

    /// <summary>Баланс на начало периода</summary>
    /// <example>100.00</example>
    public decimal OpeningBalance { get; set; }

    /// <summary>Баланс на конец периода</summary>
    /// <example>350.00</example>
    public decimal ClosingBalance { get; set; }

    /// <summary>Список транзакций за период</summary>
    public List<TransactionStatementDto>? Transactions { get; set; }

    /// <summary>Сумма всех кредитов (пополнений) за период</summary>
    /// <example>200.00</example>
    public decimal TotalCredits { get; set; }

    /// <summary>Сумма всех дебетов (списаний) за период</summary>
    /// <example>50.00</example>
    public decimal TotalDebits { get; set; }
}