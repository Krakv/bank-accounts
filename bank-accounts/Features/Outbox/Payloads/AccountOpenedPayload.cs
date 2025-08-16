namespace bank_accounts.Features.Outbox.Payloads;

public class AccountOpenedPayload : OutboxPayload
{
    public required Guid AccountId { get; set; }
    public required Guid OwnerId { get; set; }
    public required string Currency { get; set; }
    public required string Type { get; set; }
}