namespace bank_accounts.Features.Accounts.Dto;

/// <summary>
/// Данные для создания банковского счета
/// </summary>
public record CreateAccountDto
{
    /// <summary>
    /// ID владельца счета (обязательно)
    /// </summary>
    /// <example>3fa85f64-5717-4562-b3fc-2c963f66afa6</example>
    public Guid OwnerId { get; init; }

    /// <summary>
    /// Тип счета: Deposit, Checking или Credit (обязательно)
    /// </summary>
    /// <example>Deposit</example>
    public string Type { get; init; } = string.Empty;

    /// <summary>
    /// Валюта счета: RUB, USD или EUR (обязательно)
    /// </summary>
    /// <example>EUR</example>
    public string Currency { get; init; } = string.Empty;

    /// <summary>
    /// Процентная ставка (только для депозитов/кредитов)
    /// </summary>
    /// <example>3.5</example>
    public decimal? InterestRate { get; init; }
}