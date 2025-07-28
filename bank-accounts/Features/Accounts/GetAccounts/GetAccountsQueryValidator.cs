using FluentValidation;
using JetBrains.Annotations;
using bank_accounts.Services.VerificationService;
using bank_accounts.Services.CurrencyService;

namespace bank_accounts.Features.Accounts.GetAccounts;

[UsedImplicitly]
public class GetAccountsQueryValidator : AbstractValidator<GetAccountsQuery>
{
    private readonly ICurrencyService _currencyService;
    private readonly IVerificationService _verificationService;

    public GetAccountsQueryValidator(ICurrencyService currencyService, IVerificationService verificationService)
    {
        _currencyService = currencyService;
        _verificationService = verificationService;

        RuleFor(x => x.AccountFilterDto.OwnerId)
            .NotEmpty()
            .When(x => x.AccountFilterDto.OwnerId.HasValue)
            .WithMessage("Owner ID must be a valid GUID")
            .MustAsync(BeVerifiedClient)
            .When(x => x.AccountFilterDto.OwnerId.HasValue)
            .WithMessage("Client is not verified");

        RuleFor(x => x.AccountFilterDto.Type)
            .Must(BeValidAccountType)
            .When(x => !string.IsNullOrEmpty(x.AccountFilterDto.Type))
            .WithMessage("Account type must be Deposit, Checking or Credit");

        RuleFor(x => x.AccountFilterDto.Currency)
            .MustAsync(BeSupportedCurrency)
            .When(x => !string.IsNullOrEmpty(x.AccountFilterDto.Currency))
            .WithMessage("Unsupported currency code");

        RuleFor(x => x.AccountFilterDto.MinBalance)
            .GreaterThanOrEqualTo(0)
            .When(x => x.AccountFilterDto.MinBalance.HasValue)
            .WithMessage("Minimum balance cannot be negative");

        RuleFor(x => x.AccountFilterDto.MaxBalance)
            .GreaterThanOrEqualTo(0)
            .When(x => x.AccountFilterDto.MaxBalance.HasValue)
            .WithMessage("Maximum balance cannot be negative")
            .GreaterThanOrEqualTo(x => x.AccountFilterDto.MinBalance)
            .When(x => x.AccountFilterDto is { MaxBalance: not null, MinBalance: not null })
            .WithMessage("Maximum balance must be greater than or equal to minimum balance");

        RuleFor(x => x.AccountFilterDto.MinInterestRate)
            .Must((dto, rate) => BeValidInterestRate(dto.AccountFilterDto.Type, rate))
            .When(x => x.AccountFilterDto.MinInterestRate.HasValue)
            .WithMessage("Interest rate must be positive for Deposit/Credit accounts and null for Checking accounts");

        RuleFor(x => x.AccountFilterDto.MaxInterestRate)
            .Must((dto, rate) => BeValidInterestRate(dto.AccountFilterDto.Type, rate))
            .When(x => x.AccountFilterDto.MaxInterestRate.HasValue)
            .WithMessage("Interest rate must be positive for Deposit/Credit accounts and null for Checking accounts")
            .GreaterThanOrEqualTo(x => x.AccountFilterDto.MinInterestRate)
            .When(x => x.AccountFilterDto is { MaxInterestRate: not null, MinInterestRate: not null })
            .WithMessage("Maximum interest rate must be greater than or equal to minimum rate");

        RuleFor(x => x.AccountFilterDto.OpeningDateTo)
            .GreaterThanOrEqualTo(x => x.AccountFilterDto.OpeningDateFrom)
            .When(x => x.AccountFilterDto is { OpeningDateTo: not null, OpeningDateFrom: not null })
            .WithMessage("Opening date 'to' must be after or equal to 'from' date");

        RuleFor(x => x.AccountFilterDto.ClosingDateTo)
            .GreaterThanOrEqualTo(x => x.AccountFilterDto.ClosingDateFrom)
            .When(x => x.AccountFilterDto is { ClosingDateTo: not null, ClosingDateFrom: not null })
            .WithMessage("Closing date 'to' must be after or equal to 'from' date");

        RuleFor(x => x.AccountFilterDto.AccountIds)
            .Must(ids => ids == null || ids.All(id => id != Guid.Empty))
            .When(x => x.AccountFilterDto.AccountIds != null)
            .WithMessage("Account IDs must contain valid GUID values");
    }

    private async Task<bool> BeVerifiedClient(Guid? ownerId, CancellationToken ct)
    {
        return ownerId.HasValue && await _verificationService.VerifyClientAsync(ownerId.Value);
    }

    private async Task<bool> BeSupportedCurrency(string? currencyCode, CancellationToken ct)
    {
        return !string.IsNullOrEmpty(currencyCode) &&
               await _currencyService.IsCurrencySupportedAsync(currencyCode);
    }

    private static bool BeValidAccountType(string? type) =>
        type is "Deposit" or "Checking" or "Credit";

    private static bool BeValidInterestRate(string? accountType, decimal? rate)
    {
        if (accountType is "Deposit" or "Credit")
        {
            return rate is > 0 and <= 100;
        }
        return !rate.HasValue;
    }
}