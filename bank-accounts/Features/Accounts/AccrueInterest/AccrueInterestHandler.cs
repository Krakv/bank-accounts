using bank_accounts.Features.Common.UnitOfWork;
using bank_accounts.Features.Outbox;
using bank_accounts.Features.Outbox.Payloads;
using MediatR;

namespace bank_accounts.Features.Accounts.AccrueInterest;

public class AccrueInterestHandler(IUnitOfWork unitOfWork) : IRequestHandler<AccrueInterestCommand>
{
    public async Task Handle(AccrueInterestCommand request, CancellationToken cancellationToken)
    {
        await unitOfWork.BeginTransactionAsync();
        try
        {
            var dto = await unitOfWork.Accounts.AccrueInterestAsync(request.AccountId);

            if (dto != null)
            {
                var payload = new InterestAccruedPayload
                {
                    AccountId = dto.AccountId,
                    PeriodFrom = dto.PeriodFrom.ToString("O"),
                    PeriodTo = dto.PeriodTo.ToString("O"),
                    Amount = dto.Amount
                };

                var outboxMessage = OutboxMessageFactory.Create(payload.EventId, "InterestAccrued", payload, payload.Meta.Source, payload.Meta.CorrelationId, payload.Meta.CausationId);

                await unitOfWork.OutboxMessages.CreateAsync(outboxMessage);
                await unitOfWork.OutboxMessages.SaveChangesAsync();
            }

            await unitOfWork.CommitAsync();
        }
        catch (Exception)
        {
            await unitOfWork.RollbackAsync();
            throw;
        }
    }
}