namespace bank_accounts.Features.Inbox;

public class InboxPayload
{
    /// <summary>
    /// Уникальный идентификатор события
    /// </summary>
    /// <example>3fa85f64-5717-4562-b3fc-2c963f66afa7</example>
    public required Guid EventId { get; set; }

    /// <summary>
    /// Дата и время возникновения события (в UTC)
    /// </summary>
    /// <example>2023-01-01T00:00:00Z</example>
    public required DateTime OccuredAt { get; set; }

    /// <summary>
    /// Метаданные события
    /// </summary>
    public required Meta Meta { get; set; }
}