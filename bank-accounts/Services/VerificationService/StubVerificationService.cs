namespace bank_accounts.Services.VerificationService;

public class StubVerificationService : IVerificationService
{
    public Task<bool> VerifyClientAsync(Guid clientId)
    {
        return Task.FromResult(clientId != Guid.Empty);
    }
}