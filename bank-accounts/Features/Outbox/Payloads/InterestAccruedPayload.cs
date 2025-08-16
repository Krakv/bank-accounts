namespace bank_accounts.Features.Outbox.Payloads;

public class InterestAccruedPayload : OutboxPayload
{
    public required Guid AccountId { get; set; }
    public required string PeriodFrom { get; set; }
    public required string PeriodTo { get; set; }
    public required decimal Amount { get; set; }
}