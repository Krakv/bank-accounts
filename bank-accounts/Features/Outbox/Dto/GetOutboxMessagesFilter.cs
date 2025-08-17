using bank_accounts.Features.Common;
using bank_accounts.Features.Outbox.Entities;

namespace bank_accounts.Features.Outbox.Dto;

public class GetOutboxMessagesFilter : Filter<OutboxMessage>
{
    public override IQueryable<OutboxMessage> ApplyFilters(IQueryable<OutboxMessage> query)
    {
        return query.OrderBy(x => x.OccurredAt);
    }
}