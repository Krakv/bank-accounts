namespace bank_accounts.Features.Accounts.Dto
{
    /// <summary>
    /// Результат запроса списка счетов с пагинацией
    /// </summary>
    public record AccountsDto
    {
        /// <summary>
        /// Список банковских счетов
        /// </summary>
        public required IEnumerable<AccountDto>? Accounts { get; init; }

        /// <summary>
        /// Информация о пагинации (обязательно)
        /// </summary>
        public required PaginationDto Pagination { get; init; }
    }
}