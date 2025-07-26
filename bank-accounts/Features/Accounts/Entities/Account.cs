using bank_accounts.Features.Transactions.Entities;

namespace bank_accounts.Features.Accounts.Entities
{
    public class Account : IEntity
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid OwnerId { get; set; }
        public string Type { get; set; }
        public string Currency { get; set; }
        public decimal Balance { get; set; } = decimal.Zero;
        public decimal? InterestRate { get; set; }
        public DateTime OpeningDate { get; set; } = DateTime.UtcNow;
        public DateTime? ClosingDate { get; set; }
        public virtual List<Transaction>? Transactions { get; set; }
    }
}
