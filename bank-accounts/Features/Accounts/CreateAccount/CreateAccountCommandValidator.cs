using bank_accounts.Services.CurrencyService;
using bank_accounts.Services.VerificationService;
using FluentValidation;
using JetBrains.Annotations;

namespace bank_accounts.Features.Accounts.CreateAccount;

[UsedImplicitly]
public class CreateAccountCommandValidator : AbstractValidator<CreateAccountCommand>
{
    private readonly IVerificationService _verificationService;
    private readonly ICurrencyService _currencyService;

    public CreateAccountCommandValidator(IVerificationService verificationService, ICurrencyService currencyService)
    {
        _verificationService = verificationService;
        _currencyService = currencyService;

        RuleFor(x => x.CreateAccountDto.OwnerId)
            .NotEmpty().WithMessage("OwnerId is required")
            .MustAsync(BeVerifiedClient).WithMessage("Client is not verified");

        RuleFor(x => x.CreateAccountDto.Type)
            .NotEmpty().WithMessage("Account type is required")
            .Must(BeValidAccountType).WithMessage("Account type must be Deposit, Checking or Credit");

        RuleFor(x => x.CreateAccountDto.Currency)
            .NotEmpty().WithMessage("Currency is required")
            .Length(3).WithMessage("Currency code must be 3 characters")
            .MustAsync(BeSupportedCurrency).WithMessage("Unsupported currency");

        RuleFor(x => x.CreateAccountDto.InterestRate)
            .Must((dto, rate) => BeValidInterestRate(dto.CreateAccountDto.Type, rate))
            .WithMessage("Interest rate must be positive for Deposit/Credit accounts and null for Checking accounts");
    }

    private async Task<bool> BeVerifiedClient(Guid ownerId, CancellationToken cancellationToken)
    {
        return await _verificationService.VerifyClientAsync(ownerId);
    }

    private async Task<bool> BeSupportedCurrency(string currencyCode, CancellationToken ct)
    {
        return await _currencyService.IsCurrencySupportedAsync(currencyCode);
    }

    private static bool BeValidAccountType(string type)
    {
        return type is "Deposit" or "Checking" or "Credit";
    }

    private static bool BeValidInterestRate(string accountType, decimal? rate)
    {
        if (accountType is "Deposit" or "Credit")
        {
            return rate is >= 0 and <= 100;
        }

        return !rate.HasValue;
    }
}