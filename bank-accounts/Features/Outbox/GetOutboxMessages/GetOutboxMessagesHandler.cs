using bank_accounts.Exceptions;
using bank_accounts.Features.Outbox.Entities;
using bank_accounts.Infrastructure.Repository;
using MediatR;

namespace bank_accounts.Features.Outbox.GetOutboxMessages;

public class GetOutboxMessagesHandler(IRepository<OutboxMessage> outboxMessageRepository)
    : IRequestHandler<GetOutboxMessagesQuery, OutboxMessage[]>
{
    public async Task<OutboxMessage[]> Handle(GetOutboxMessagesQuery request, CancellationToken cancellationToken)
    {
        var filter = request.OutboxMessageFilterDto;

        var (messages, totalCount) = await outboxMessageRepository.GetFilteredAsync(filter);

        if (totalCount == 0)
        {
            throw new NotFoundAppException("No outbox messages found");
        }

        return messages.ToArray();
    }
}