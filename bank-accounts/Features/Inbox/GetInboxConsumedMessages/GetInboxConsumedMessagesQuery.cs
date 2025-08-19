using bank_accounts.Features.Inbox.Dto;
using bank_accounts.Features.Inbox.Entities;
using MediatR;

namespace bank_accounts.Features.Inbox.GetInboxConsumedMessages;

public class GetInboxConsumedMessagesQuery(InboxConsumedMessagesFilter consumedMessageFilterDto) : IRequest<InboxConsumedMessage[]>
{
    public InboxConsumedMessagesFilter ConsumedMessageFilterDto { get; } = consumedMessageFilterDto;
}