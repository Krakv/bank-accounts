using System.ComponentModel.DataAnnotations;

namespace bank_accounts.Features.Accounts.Dtos
{
    public class UpdateInterestRateDto
    {
        [Range(0, 100, ErrorMessage = "Interest rate must be between 0 and 100.")]
        public decimal? InterestRate { get; set; }
    }
}
