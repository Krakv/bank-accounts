using bank_accounts.Features.Transactions.Models;

namespace bank_accounts.Features.Accounts.Models
{
    public class Account
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid OwnerId { get; set; }
        public required string Type { get; set; }
        public required string Currency { get; set; }
        public decimal Balance { get; set; } = decimal.Zero;
        public decimal? InterestRate { get; set; }
        public DateTime OpeningDate { get; set; } = DateTime.UtcNow;
        public DateTime? ClosingDate { get; set; }
        public virtual List<Transaction>? Transactions { get; set; }
    }
}
