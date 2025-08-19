using bank_accounts.Exceptions;
using bank_accounts.Features.Inbox.Entities;
using bank_accounts.Infrastructure.Repository;
using MediatR;

namespace bank_accounts.Features.Inbox.GetInboxDeadMessages;

public class GetDeadMessagesHandler(IRepository<InboxDeadMessage> deadMessageRepository) : IRequestHandler<GetInboxDeadMessagesQuery, InboxDeadMessage[]>
{
    public async Task<InboxDeadMessage[]> Handle(GetInboxDeadMessagesQuery request, CancellationToken cancellationToken)
    {
        var filter = request.DeadMessageFilterDto;

        var (messages, totalCount) = await deadMessageRepository.GetFilteredAsync(filter);

        if (totalCount == 0)
        {
            throw new NotFoundAppException("No dead messages found");
        }

        return messages.ToArray();
    }
}