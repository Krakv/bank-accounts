namespace bank_accounts.Features.Accounts.Dtos
{
    public class AccountDto
    {
        public Guid Id { get; set; }
        public Guid OwnerId { get; set; }
        public required string Type { get; set; }
        public required string Currency { get; set; }
        public decimal Balance { get; set; }
        public decimal? InterestRate { get; set; }
        public DateTime OpeningDate { get; set; }
        public DateTime? ClosingDate { get; set; }
    }
}
