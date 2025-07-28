namespace bank_accounts.Services.CurrencyService;

public class StubCurrencyService : ICurrencyService
{
    private readonly HashSet<string> _supportedCurrencies = ["RUB", "USD", "EUR"];

    public Task<bool> IsCurrencySupportedAsync(string currencyCode)
    {
        var isSupported = _supportedCurrencies.Contains(currencyCode);
        return Task.FromResult(isSupported);
    }
}