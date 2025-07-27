namespace bank_accounts.Features.Accounts.Dto
{
    /// <summary>
    /// DTO для пагинации результатов запроса
    /// </summary>
    /// <remarks>
    /// Содержит информацию о текущей странице, размере страницы и общем количестве элементов.
    /// </remarks>
    public record PaginationDto
    {
        /// <summary>
        /// Текущий номер страницы (начинается с 1)
        /// </summary>
        /// <example>1</example>
        public required int Page { get; init; }

        /// <summary>
        /// Количество элементов на странице
        /// </summary>
        /// <example>20</example>
        public required int PageSize { get; init; }

        /// <summary>
        /// Общее количество элементов во всех страницах
        /// </summary>
        /// <example>100</example>
        public required int TotalCount { get; init; }

        /// <summary>
        /// Общее количество страниц
        /// </summary>
        /// <example>5</example>
        public required int TotalPages { get; init; }
    }
}