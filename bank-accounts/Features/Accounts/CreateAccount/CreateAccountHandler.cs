using bank_accounts.Features.Accounts.Entities;
using bank_accounts.Features.Outbox;
using MediatR;
using bank_accounts.Features.Outbox.Payloads;
using bank_accounts.Features.Common.UnitOfWork;

namespace bank_accounts.Features.Accounts.CreateAccount;

public class CreateAccountHandler(IUnitOfWork unitOfWork) : IRequestHandler<CreateAccountCommand, Guid>
{
    public async Task<Guid> Handle(CreateAccountCommand request, CancellationToken cancellationToken)
    {
        var accountDto = request.CreateAccountDto;
        var account = new Account
        {
            OwnerId = accountDto.OwnerId,
            Type = accountDto.Type,
            Currency = accountDto.Currency,
            InterestRate = accountDto.InterestRate
        };

        await unitOfWork.BeginTransactionAsync();

        try
        {
            await unitOfWork.Accounts.CreateAsync(account);
            await unitOfWork.Accounts.SaveChangesAsync();

            var payload = new AccountOpenedPayload
            {
                AccountId = account.Id, 
                OwnerId = account.OwnerId, 
                Currency = account.Currency, 
                Type = account.Type
            };

            var outboxMessage = OutboxMessageFactory.Create(payload.EventId, "AccountOpened", payload, payload.Meta.Source, payload.Meta.CorrelationId, payload.Meta.CausationId);

            await unitOfWork.OutboxMessages.CreateAsync(outboxMessage);
            await unitOfWork.OutboxMessages.SaveChangesAsync();

            await unitOfWork.CommitAsync();

            return account.Id;
        }
        catch (Exception)
        {
            await unitOfWork.RollbackAsync();
            throw;
        }
    }
}