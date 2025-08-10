namespace bank_accounts.Services.RecurringOperationsService;

public interface IAccrueInterestService
{
    Task AccrueInterestForAllAccountsAsync();
}