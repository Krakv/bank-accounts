namespace bank_accounts.Services.AccrueInterestService;

public interface IAccrueInterestService
{
    Task AccrueInterestForAllAccountsAsync();
}