using bank_accounts.Features.Common;
using bank_accounts.Features.Outbox.Entities;

namespace bank_accounts.Features.Outbox.Dto;

public class OutboxFilterDto : Filter<OutboxMessage>
{
    public override IQueryable<OutboxMessage> ApplyFilters(IQueryable<OutboxMessage> query)
    {
        return query.Where(x => x.ProcessedAt == null)
            .OrderBy(x => x.OccurredAt);
    }
}