
using bank_accounts.Features.Accounts.Dto;
using bank_accounts.Features.Accounts.Entities;
using bank_accounts.Infrastructure.Repository;

namespace bank_accounts.Services.RecurringOperationsService;

public class AccrueInterestService(IRepository<Account> accountRepository, ILogger<AccrueInterestService> logger) : IAccrueInterestService
{
    public async Task AccrueInterestForAllAccountsAsync()
    {
        try
        {
            var filter = new AccountFilterDto { MinInterestRate = 1, Type = "Deposit" };
            
            var depositAccounts = (await accountRepository
                .GetFilteredAsync(filter)).data;

            foreach (var account in depositAccounts)
            {
                try
                {
                    await accountRepository.AccrueInterestAsync(account.Id);
                    logger.LogInformation(
                        "Начислены проценты по счету {AccountId}",
                        account.Id);
                }
                catch (Exception ex)
                {
                    logger.LogError(ex,
                        "Ошибка при начислении процентов по счету {AccountId}",
                        account.Id);
                }
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Ошибка в сервисе начисления процентов");
            throw;
        }
    }
}