namespace bank_accounts.Features.Accounts.Dtos
{
    public class AccountStatementResponseDto
    {
        public Guid AccountId { get; set; }
        public Guid OwnerId { get; set; }
        public string Currency { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public decimal OpeningBalance { get; set; }
        public decimal ClosingBalance { get; set; }
        public List<TransactionStatementDto> Transactions { get; set; }
        public decimal TotalCredits { get; set; }
        public decimal TotalDebits { get; set; }
    }
}
