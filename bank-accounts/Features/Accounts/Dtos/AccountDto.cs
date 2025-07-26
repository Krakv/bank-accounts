namespace bank_accounts.Features.Accounts.Dtos
{
    public record AccountDto(
        Guid Id,
        Guid OwnerId,
        string Type,
        string Currency,
        decimal Balance,
        DateTime OpeningDate,
        decimal? InterestRate = null,
        DateTime? ClosingDate = null
    );
}
