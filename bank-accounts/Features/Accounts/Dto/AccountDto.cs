namespace bank_accounts.Features.Accounts.Dto;

/// <summary>
/// Данные банковского счета
/// </summary>
public record AccountDto
{
    /// <summary>
    /// Уникальный идентификатор счета
    /// </summary>
    /// <example>3fa85f64-5717-4562-b3fc-2c963f66afa6</example>
    public Guid Id { get; init; }

    /// <summary>
    /// Идентификатор владельца счета
    /// </summary>
    /// <example>3fa85f64-5717-4562-b3fc-2c963f66afa6</example>
    public Guid OwnerId { get; init; }

    /// <summary>
    /// Тип счета (Deposit - депозитный, Checking - обычный счет, Credit - кредитный)
    /// </summary>
    /// <example>Deposit</example>
    public string Type { get; init; } = string.Empty;

    /// <summary>
    /// Валюта счета в формате ISO 4217
    /// </summary>
    /// <example>USD</example>
    public string Currency { get; init; } = string.Empty;

    /// <summary>
    /// Текущий баланс счета
    /// </summary>
    /// <example>1500.50</example>
    public decimal Balance { get; init; }

    /// <summary>
    /// Дата открытия счета
    /// </summary>
    /// <example>2023-01-15</example>
    public DateTime OpeningDate { get; init; }

    /// <summary>
    /// Процентная ставка (только для депозитных и кредитных счетов)
    /// </summary>
    /// <example>3.5</example>
    public decimal? InterestRate { get; init; }

    /// <summary>
    /// Дата закрытия счета (null если счет активен)
    /// </summary>
    /// <example>null</example>
    public DateTime? ClosingDate { get; init; }

    /// <summary>
    /// Флаг, отображающий состояние счета (заморожен/не заморожен)
    /// </summary>
    /// <example>false</example>
    public bool IsFrozen { get; init; } = false;
}