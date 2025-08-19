using bank_accounts.Features.Common;
using bank_accounts.Features.Inbox.Entities;

namespace bank_accounts.Features.Inbox.Dto;

public class InboxConsumedMessagesFilter : Filter<InboxConsumedMessage>
{
    public override IQueryable<InboxConsumedMessage> ApplyFilters(IQueryable<InboxConsumedMessage> query)
    {
        return query.OrderBy(x => x.ProcessedAt);
    }
}