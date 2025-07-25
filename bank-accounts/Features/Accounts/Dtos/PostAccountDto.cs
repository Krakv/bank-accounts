using System.ComponentModel.DataAnnotations;

namespace bank_accounts.Features.Accounts.Dtos
{
    public class PostAccountDto
    {
        [Required(ErrorMessage = "OwnerId is required.")]
        public Guid OwnerId { get; set; }

        [Required(ErrorMessage = "Account type is required.")]
        public string Type { get; set; } = "Checking";

        [Required(ErrorMessage = "Currency is required.")]

        public string Currency { get; set; } = "RUB";

        [Range(0, 100, ErrorMessage = "Interest rate must be between 0 and 100.")]
        public decimal? InterestRate { get; set; }
    }
}
