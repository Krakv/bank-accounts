using bank_accounts.Features.Abstract;
using bank_accounts.Features.Accounts.Entities;

namespace bank_accounts.Features.Accounts.Dto;

/// <summary>
/// DTO для фильтрации списка счетов
/// </summary>
/// <remarks>
/// <para>Поддерживает фильтрацию по всем основным параметрам счетов.</para>
/// <para>Все параметры являются опциональными.</para>
/// </remarks>
public class AccountFilterDto : Filter<Account>
{
    /// <summary>
    /// ID владельца счета
    /// </summary>
    /// <example>3fa85f64-5717-4562-b3fc-2c963f66afa6</example>
    public Guid? OwnerId { get; set; }

    /// <summary>
    /// Тип счета (Deposit, Current, Credit)
    /// </summary>
    /// <example>Deposit</example>
    public string? Type { get; set; }

    /// <summary>
    /// Валюта счета (ISO код: RUB, USD, EUR)
    /// </summary>
    /// <example>USD</example>
    public string? Currency { get; set; }

    /// <summary>
    /// Минимальный баланс счета
    /// </summary>
    /// <example>1000</example>
    public decimal? MinBalance { get; set; }

    /// <summary>
    /// Максимальный баланс счета
    /// </summary>
    /// <example>5000</example>
    public decimal? MaxBalance { get; set; }

    /// <summary>
    /// Минимальная процентная ставка (для депозитов/кредитов)
    /// </summary>
    /// <example>3.5</example>
    public decimal? MinInterestRate { get; set; }

    /// <summary>
    /// Максимальная процентная ставка (для депозитов/кредитов)
    /// </summary>
    /// <example>5.0</example>
    public decimal? MaxInterestRate { get; set; }

    /// <summary>
    /// Дата открытия счета (от)
    /// </summary>
    /// <example>2023-01-01</example>
    public DateTime? OpeningDateFrom { get; set; }

    /// <summary>
    /// Дата открытия счета (до)
    /// </summary>
    /// <example>2023-12-31</example>
    public DateTime? OpeningDateTo { get; set; }

    /// <summary>
    /// Дата закрытия счета (от)
    /// </summary>
    /// <example>2023-06-01</example>
    public DateTime? ClosingDateFrom { get; set; }

    /// <summary>
    /// Дата закрытия счета (до)
    /// </summary>
    /// <example>2023-06-30</example>
    public DateTime? ClosingDateTo { get; set; }

    /// <summary>
    /// Фильтр по активности счетов
    /// </summary>
    /// <remarks>
    /// true - только активные счета (ClosingDate == null)
    /// false - только закрытые счета
    /// null - все счета
    /// </remarks>
    /// <example>true</example>
    public bool? IsActive { get; set; }

    /// <summary>
    /// Список ID счетов для фильтрации
    /// </summary>
    /// <example>["3fa85f64-5717-4562-b3fc-2c963f66afa6", "3fa85f64-5717-4562-b3fc-2c963f66afa7"]</example>
    public List<Guid>? AccountIds { get; set; }

    /// <summary>
    /// Применяет заданные фильтры к запросу
    /// </summary>
    /// <param name="query">Исходный IQueryable</param>
    /// <returns>IQueryable с примененными фильтрами</returns>
    public override IQueryable<Account> ApplyFilters(IQueryable<Account> query)
    {
        if (OwnerId.HasValue)
            query = query.Where(a => a.OwnerId == OwnerId.Value);

        if (!string.IsNullOrEmpty(Type))
            query = query.Where(a => a.Type == Type);

        if (!string.IsNullOrEmpty(Currency))
            query = query.Where(a => a.Currency == Currency);

        if (MinBalance.HasValue)
            query = query.Where(a => a.Balance >= MinBalance.Value);

        if (MaxBalance.HasValue)
            query = query.Where(a => a.Balance <= MaxBalance.Value);

        if (MinInterestRate.HasValue)
            query = query.Where(a => a.InterestRate >= MinInterestRate.Value);

        if (MaxInterestRate.HasValue)
            query = query.Where(a => a.InterestRate <= MaxInterestRate.Value);

        if (OpeningDateFrom.HasValue)
            query = query.Where(a => a.OpeningDate >= OpeningDateFrom.Value);

        if (OpeningDateTo.HasValue)
            query = query.Where(a => a.OpeningDate <= OpeningDateTo.Value);

        if (ClosingDateFrom.HasValue)
            query = query.Where(a => a.ClosingDate >= ClosingDateFrom.Value);

        if (ClosingDateTo.HasValue)
            query = query.Where(a => a.ClosingDate <= ClosingDateTo.Value);

        if (IsActive.HasValue)
            query = IsActive.Value
                ? query.Where(a => a.ClosingDate == null)
                : query.Where(a => a.ClosingDate != null);

        if (AccountIds?.Count > 0)
            query = query.Where(a => AccountIds.Contains(a.Id));

        return query;
    }
}