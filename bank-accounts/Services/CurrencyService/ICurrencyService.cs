namespace bank_accounts.Services.CurrencyService;

public interface ICurrencyService
{
    Task<bool> IsCurrencySupportedAsync(string currencyCode);
}