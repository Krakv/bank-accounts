namespace bank_accounts.Features.Accounts.Dtos
{
    public record PaginationDto
    {
        public required int Page { get; init; }
        public required int PageSize { get; init; }
        public required int TotalCount { get; init; }
        public required int TotalPages { get; init; }
    }
}
