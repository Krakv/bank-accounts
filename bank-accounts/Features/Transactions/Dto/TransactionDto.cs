namespace bank_accounts.Features.Transactions.Dto;

/// <summary>
/// Данные о транзакции
/// </summary>
public class TransactionDto
{
    /// <summary>Уникальный идентификатор транзакции</summary>
    /// <example>3fa85f64-5717-4562-b3fc-2c963f66afa6</example>
    public Guid TransactionId { get; set; }

    /// <summary>Идентификатор счета</summary>
    /// <example>3fa85f64-5717-4562-b3fc-2c963f66afa6</example>
    public Guid AccountId { get; set; }

    /// <summary>Идентификатор счета-контрагента (null для депозитов/снятий)</summary>
    /// <example>3fa85f64-5717-4562-b3fc-2c963f66afa6</example>
    public Guid? CounterpartyAccountId { get; set; }

    /// <summary>Валюта транзакции (ISO код)</summary>
    /// <example>EUR</example>
    public string Currency { get; set; } = string.Empty;

    /// <summary>Сумма транзакции</summary>
    /// <example>100.00</example>
    public decimal Value { get; set; }

    /// <summary>Тип транзакции (Credit - зачисление, Debit - списание)</summary>
    /// <example>Credit</example>
    public string Type { get; set; } = string.Empty;

    /// <summary>Описание транзакции</summary>
    /// <example>Перевод между счетами</example>
    public string Description { get; set; } = string.Empty;

    /// <summary>Дата и время транзакции</summary>
    /// <example>2025-03-12T11:30:19</example>
    public DateTime Date { get; set; }
}