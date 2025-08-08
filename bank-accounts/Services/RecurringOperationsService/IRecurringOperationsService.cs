namespace bank_accounts.Services.RecurringOperationsService;

public interface IRecurringOperationsService
{
    Task AccrueDepositInterest();
}