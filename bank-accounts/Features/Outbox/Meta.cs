using JetBrains.Annotations;

namespace bank_accounts.Features.Outbox;

public class Meta
{
    [UsedImplicitly] // Не используется напрямую в коде
    public string Version { get; set; } = "v1";
    public string Source { get; set; } = "account-service";
    public Guid CorrelationId { get; set; } = Guid.NewGuid();
    public Guid CausationId { get; set; } = Guid.NewGuid();
}