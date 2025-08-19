using bank_accounts.Features.Outbox.Dto;
using bank_accounts.Features.Outbox.Entities;
using MediatR;

namespace bank_accounts.Features.Outbox.GetOutboxMessages;

public class GetOutboxMessagesQuery(GetOutboxMessagesFilter outboxMessageFilterDto) : IRequest<OutboxMessage[]>
{
    public GetOutboxMessagesFilter OutboxMessageFilterDto { get; } = outboxMessageFilterDto;
}