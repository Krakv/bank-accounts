using FluentValidation;

namespace bank_accounts.Features.Accounts.CreateAccount
{
    public class CreateAccountCommandValidator : AbstractValidator<CreateAccountCommand>
    {
        public CreateAccountCommandValidator()
        {
            RuleFor(x => x.CreateAccountDto.OwnerId)
                .NotEmpty().WithMessage("OwnerId is required");

            RuleFor(x => x.CreateAccountDto.Type)
                .NotEmpty().WithMessage("Account type is required")
                .Must(BeValidAccountType).WithMessage("Account type must be Deposit, Checking or Credit");

            RuleFor(x => x.CreateAccountDto.Currency)
                .NotEmpty().WithMessage("Currency is required")
                .Length(3).WithMessage("Currency code must be 3 characters")
                .Must(BeValidCurrencyCode).WithMessage("Invalid currency code (ISO 4217)");

            RuleFor(x => x.CreateAccountDto.InterestRate)
                .Must((dto, rate) => BeValidInterestRate(dto.CreateAccountDto.Type, rate))
                .WithMessage("Interest rate must be positive for Deposit/Credit accounts and null for Checking accounts");
        }

        private bool BeValidAccountType(string type)
        {
            return type is "Deposit" or "Checking" or "Credit";
        }

        private bool BeValidCurrencyCode(string currency)
        {
            return currency is "RUB" or "EUR" or "USD";
        }

        private bool BeValidInterestRate(string accountType, decimal? rate)
        {
            if (accountType is "Deposit" or "Credit")
            {
                return rate.HasValue && rate >= 0 && rate <= 100;
            }

            return !rate.HasValue;
        }
    }
}
