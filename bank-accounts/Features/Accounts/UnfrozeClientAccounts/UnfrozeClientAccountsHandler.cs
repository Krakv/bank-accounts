using bank_accounts.Features.Accounts.Dto;
using bank_accounts.Features.Common.UnitOfWork;
using bank_accounts.Features.Inbox.Entities;
using MediatR;

namespace bank_accounts.Features.Accounts.UnfrozeClientAccounts;

public class UnfrozeClientAccountsHandler(IUnitOfWork unitOfWork) : IRequestHandler<UnfrozeClientAccountsCommand, bool>
{
    public async Task<bool> Handle(UnfrozeClientAccountsCommand request, CancellationToken cancellationToken)
    {
        await unitOfWork.BeginTransactionAsync();
        try
        {
            if (request.EventPayload != null)
            {
                var inboxConsumedMessage = new InboxConsumedMessage
                {
                    Handler = nameof(UnfrozeClientAccountsHandler),
                    Id = request.EventPayload.EventId,
                    ProcessedAt = DateTime.UtcNow

                };

                await unitOfWork.InboxConsumedMessages.CreateAsync(inboxConsumedMessage);
                await unitOfWork.InboxConsumedMessages.SaveChangesAsync();
            }

            var filter = new AccountFilterDto { Page = 1, PageSize = 10000, OwnerId = request.ClientId };

            var accounts = (await unitOfWork.Accounts.GetFilteredAsync(filter)).data;

            foreach (var account in accounts)
            {
                account.IsFrozen = false;
            }

            await unitOfWork.Accounts.SaveChangesAsync();

            await unitOfWork.CommitAsync();

            return accounts.Count != 0;
        }
        catch (Exception)
        {
            await unitOfWork.RollbackAsync();
            throw;
        }
    }
}