using System.ComponentModel.DataAnnotations;

namespace bank_accounts.Features.Accounts.Dtos
{
    public class AccountStatementRequestDto
    {
        [Required]
        public DateTime StartDate { get; set; }

        [Required]
        public DateTime EndDate { get; set; }
    }
}
