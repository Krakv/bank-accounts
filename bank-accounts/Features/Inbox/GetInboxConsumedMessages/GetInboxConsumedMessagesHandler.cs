using bank_accounts.Exceptions;
using bank_accounts.Features.Inbox.Entities;
using bank_accounts.Infrastructure.Repository;
using MediatR;

namespace bank_accounts.Features.Inbox.GetInboxConsumedMessages;

public class GetInboxConsumedMessagesHandler(IRepository<InboxConsumedMessage> consumedMessageRepository) : IRequestHandler<GetInboxConsumedMessagesQuery, InboxConsumedMessage[]>
{
    public async Task<InboxConsumedMessage[]> Handle(GetInboxConsumedMessagesQuery request, CancellationToken cancellationToken)
    {
        var filter = request.ConsumedMessageFilterDto;

        var (messages, totalCount) = await consumedMessageRepository.GetFilteredAsync(filter);

        if (totalCount == 0)
        {
            throw new NotFoundAppException("No consumed messages found");
        }

        return messages.ToArray();
    }
}