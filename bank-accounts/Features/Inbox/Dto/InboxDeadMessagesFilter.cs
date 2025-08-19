using bank_accounts.Features.Common;
using bank_accounts.Features.Inbox.Entities;

namespace bank_accounts.Features.Inbox.Dto;

public class InboxDeadMessagesFilter : Filter<InboxDeadMessage>
{
    public override IQueryable<InboxDeadMessage> ApplyFilters(IQueryable<InboxDeadMessage> query)
    {
        return query.OrderBy(x => x.ReceivedAt);
    }
}