namespace bank_accounts.Features.Accounts.Dtos
{
    public record AccountsDto
    {
        public required IEnumerable<AccountDto>? Accounts { get; init; }
        public required PaginationDto Pagination { get; init; }
    }
}
