using bank_accounts.Features.Transactions.Entities;
using System.ComponentModel.DataAnnotations;

namespace bank_accounts.Features.Accounts.Entities
{
    public class Account : IEntity
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid OwnerId { get; set; }

        [MinLength(6)]
        [MaxLength(8)]
        public string Type { get; set; } = string.Empty;

        [StringLength(3)]
        public string Currency { get; set; } = string.Empty;
        public decimal Balance { get; set; } = decimal.Zero;
        public decimal? InterestRate { get; set; }
        public DateTime OpeningDate { get; set; } = DateTime.UtcNow;
        public DateTime? ClosingDate { get; set; }
        public virtual List<Transaction>? Transactions { get; set; }
    }
}
