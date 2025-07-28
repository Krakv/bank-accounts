namespace bank_accounts.Services.VerificationService;

public interface IVerificationService
{
    Task<bool> VerifyClientAsync(Guid clientId);
}