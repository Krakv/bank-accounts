namespace bank_accounts.Features.Accounts.Dtos
{
    public record CreateAccountDto(
        Guid OwnerId,
        string Type,
        string Currency,
        decimal? InterestRate
    );
}
