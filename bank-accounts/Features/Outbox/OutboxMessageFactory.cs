using System.Text.Json;
using bank_accounts.Features.Outbox.Entities;

namespace bank_accounts.Features.Outbox;

public class OutboxMessageFactory
{
    public static OutboxMessage Create(Guid eventId, string type, object payload, string source, Guid correlationId, Guid causationId)
    {
        return new OutboxMessage
        {
            Id = eventId,
            Type = type,
            OccurredAt = DateTime.UtcNow,
            Source = source,
            CorrelationId = correlationId,
            CausationId = causationId,
            Payload = JsonSerializer.Serialize(payload)
        };
    }
}