namespace bank_accounts.Features.Accounts.Dto
{
    /// <summary>
    /// Данные банковского счета, которые требуется обновить
    /// </summary>
    public class UpdateAccountDto
    {
        /// <summary>
        /// Процентная ставка (только для депозитных и кредитных счетов)
        /// </summary>
        /// <example>3.5</example>
        public decimal? InterestRate { get; set; }
    }
}
