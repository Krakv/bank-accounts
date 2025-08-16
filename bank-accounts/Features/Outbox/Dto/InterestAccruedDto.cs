namespace bank_accounts.Features.Outbox.Dto;

public class InterestAccruedDto
{
    public Guid AccountId { get; set; }
    public DateTime PeriodFrom { get; set; }
    public DateTime PeriodTo { get; set; }
    public decimal Amount { get; set; }
}