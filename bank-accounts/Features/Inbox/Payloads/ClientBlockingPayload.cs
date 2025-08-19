namespace bank_accounts.Features.Inbox.Payloads;

public class ClientBlockingPayload : InboxPayload
{
    /// <summary>
    /// Уникальный идентификатор клиента, которого блокируют/разблокируют
    /// </summary>
    /// <example>3fa85f64-5717-4562-b3fc-2c963f66afa6</example>
    public required Guid ClientId { get; set; }
}