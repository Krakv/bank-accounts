
using JetBrains.Annotations;

namespace bank_accounts.Features.Outbox;

public class OutboxPayload
{
    public Guid EventId { get; set; } = Guid.NewGuid();

    [UsedImplicitly] // Не используется напрямую в коде
    public string OccuredAt { get; set; } = DateTime.UtcNow.ToString("O");

    public Meta Meta { get; set; } = new();
}