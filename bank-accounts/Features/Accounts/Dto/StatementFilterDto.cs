using bank_accounts.Features.Abstract;
using bank_accounts.Features.Transactions.Entities;

namespace bank_accounts.Features.Accounts.Dto
{
    /// <summary>
    /// Фильтр для получения выписки по транзакциям счета
    /// </summary>
    public class StatementFilterDto : Filter<Transaction>
    {
        /// <summary>
        /// Идентификатор счета (обязательный)
        /// </summary>
        /// <example>3fa85f64-5717-4562-b3fc-2c963f66afa6</example>
        public Guid AccountId { get; set; }

        /// <summary>
        /// Начальная дата периода (включительно)
        /// </summary>
        /// <example>2025-01-01</example>
        public DateTime StartDate { get; set; }

        /// <summary>
        /// Конечная дата периода (невключительно)
        /// </summary>
        /// <example>2025-01-31</example>
        public DateTime EndDate { get; set; }

        /// <summary>
        /// Применяет фильтрацию к запросу транзакций
        /// </summary>
        /// <param name="query">Исходный запрос транзакций</param>
        /// <returns>Отфильтрованный запрос по указанному счету и периоду</returns>
        public override IQueryable<Transaction> ApplyFilters(IQueryable<Transaction> query)
        {
            return query.Where(t =>
                t.AccountId == AccountId &&
                t.Date >= StartDate &&
                t.Date <= EndDate);
        }
    }
}