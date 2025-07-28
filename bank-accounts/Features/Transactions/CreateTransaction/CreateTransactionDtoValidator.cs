using bank_accounts.Features.Transactions.Dto;
using bank_accounts.Services.CurrencyService;
using FluentValidation;
using JetBrains.Annotations;

namespace bank_accounts.Features.Transactions.CreateTransaction;

[UsedImplicitly]
public class CreateTransactionDtoValidator : AbstractValidator<CreateTransactionDto>
{
    private readonly ICurrencyService _currencyService;

    public CreateTransactionDtoValidator(ICurrencyService currencyService)
    {
        _currencyService = currencyService;

        RuleFor(x => x.AccountId)
            .NotEmpty()
            .WithMessage("Account ID is required");

        RuleFor(x => x.Currency)
            .NotEmpty()
            .WithMessage("Currency is required")
            .Length(3)
            .WithMessage("Currency code must be 3 characters")
            .MustAsync(BeSupportedCurrency)
            .WithMessage("Unsupported currency");

        RuleFor(x => x.Value)
            .GreaterThan(0)
            .WithMessage("Transaction amount must be positive");

        RuleFor(x => x.Type)
            .NotEmpty()
            .WithMessage("Transaction type is required")
            .Must(BeValidTransactionType)
            .WithMessage("Type must be 'Credit' or 'Debit'");

        RuleFor(x => x.Description)
            .MaximumLength(500)
            .WithMessage("Description cannot exceed 500 characters");

        When(x => x.CounterpartyAccountId.HasValue, () =>
        {
            RuleFor(x => x.CounterpartyAccountId)
                .NotEqual(x => x.AccountId)
                .WithMessage("Cannot transfer to the same account");
        });
    }

    private async Task<bool> BeSupportedCurrency(string currencyCode, CancellationToken ct)
    {
        return await _currencyService.IsCurrencySupportedAsync(currencyCode);
    }

    private static bool BeValidTransactionType(string type)
    {
        return type is "Credit" or "Debit";
    }
}