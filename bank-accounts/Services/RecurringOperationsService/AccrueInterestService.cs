
using bank_accounts.Features.Accounts.AccrueInterest;
using bank_accounts.Features.Accounts.Dto;
using bank_accounts.Features.Accounts.GetAccounts;
using MediatR;

namespace bank_accounts.Services.RecurringOperationsService;

public class AccrueInterestService(IMediator mediator, ILogger<AccrueInterestService> logger) : IAccrueInterestService
{
    public async Task AccrueInterestForAllAccountsAsync()
    {
        try
        {
            var filter = new AccountFilterDto { MinInterestRate = (decimal)0.01, Type = "Deposit" };

            var depositAccounts = (await mediator.Send(new GetAccountsQuery(filter))).Accounts;

            if (depositAccounts != null)
            {
                foreach (var account in depositAccounts)
                {
                    await mediator.Send(new AccrueInterestCommand(account.Id));
                }
            }
        }
        catch(Exception ex)
        {
            logger.LogError(ex, "AccrueInterest operation: error occurred.");
        }
    }
}