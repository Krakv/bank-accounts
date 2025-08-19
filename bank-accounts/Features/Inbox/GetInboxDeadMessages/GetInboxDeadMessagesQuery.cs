using bank_accounts.Features.Inbox.Dto;
using bank_accounts.Features.Inbox.Entities;
using MediatR;

namespace bank_accounts.Features.Inbox.GetInboxDeadMessages;

public class GetInboxDeadMessagesQuery(InboxDeadMessagesFilter deadMessageFilterDto) : IRequest<InboxDeadMessage[]>
{
    public InboxDeadMessagesFilter DeadMessageFilterDto { get; } = deadMessageFilterDto;
}