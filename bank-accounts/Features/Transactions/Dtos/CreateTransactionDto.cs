using System.ComponentModel.DataAnnotations;

namespace bank_accounts.Features.Transactions.Dtos
{
    public class CreateTransactionDto
    {
        [Required]
        public Guid AccountId { get; set; }

        public Guid? CounterpartyAccountId { get; set; }

        [Required]
        [StringLength(3, MinimumLength = 3)]
        public string Currency { get; set; }

        [Required]
        [Range(0.01, double.MaxValue)]
        public decimal Value { get; set; }

        [Required]
        public string Type { get; set; }

        public string Description { get; set; } = string.Empty;
    }
}
