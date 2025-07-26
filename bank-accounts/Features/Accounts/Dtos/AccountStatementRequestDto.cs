namespace bank_accounts.Features.Accounts.Dtos
{
    public class AccountStatementRequestDto
    {
        public DateTime StartDate { get; set; }

        public DateTime EndDate { get; set; }
    }
}
