namespace bank_accounts.Features.Accounts.Dtos
{
    public class TransactionStatementDto
    {
        public Guid Id { get; set; }
        public required string Type { get; set; }
        public decimal Value { get; set; }
        public string Description { get; set; } = string.Empty;
        public DateTime Date { get; set; }
        public Guid? CounterpartyAccountId { get; set; }
    }
}
