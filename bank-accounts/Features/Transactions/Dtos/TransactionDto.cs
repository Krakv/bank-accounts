namespace bank_accounts.Features.Transactions.Dtos
{
    public class TransactionDto
    {
        public Guid TransactionId { get; set; }
        public Guid AccountId { get; set; }
        public Guid? CounterpartyAccountId { get; set; }
        public string Currency { get; set; }
        public decimal Value { get; set; }
        public string Type { get; set; }
        public string Description { get; set; }
        public DateTime Date { get; set; }
    }
}
