namespace bank_accounts.Features.Transactions.Dto;

/// <summary>
/// DTO для создания новой транзакции
/// </summary>
public class CreateTransactionDto
{
    /// <summary>
    /// Идентификатор основного счета (обязательно)
    /// </summary>
    /// <example>3fa85f64-5717-4562-b3fc-2c963f66afa6</example>
    public Guid AccountId { get; set; }

    /// <summary>
    /// Идентификатор счета-контрагента (обязательно только для переводов)
    /// </summary>
    /// <example>3fa85f64-5717-4562-b3fc-2c963f66afa7</example>
    public Guid? CounterpartyAccountId { get; set; }

    /// <summary>
    /// Валюта транзакции (ISO код: RUB, USD, EUR) (обязательно)
    /// </summary>
    /// <example>EUR</example>
    public string Currency { get; set; } = string.Empty;

    /// <summary>
    /// Сумма транзакции (должна быть больше 0) (обязательно)
    /// </summary>
    /// <example>100.50</example>
    public decimal Value { get; set; }

    /// <summary>
    /// Тип операции: Credit - зачисление, Debit - списание (обязательно)
    /// </summary>
    /// <example>Credit</example>
    public string Type { get; set; } = string.Empty;

    /// <summary>
    /// Описание транзакции (макс. 255 символов)
    /// </summary>
    /// <example>Перевод между счетами</example>
    public string Description { get; set; } = string.Empty;
}