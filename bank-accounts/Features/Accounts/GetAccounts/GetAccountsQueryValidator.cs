using FluentValidation;

namespace bank_accounts.Features.Accounts.GetAccounts
{
    public class GetAccountsQueryValidator : AbstractValidator<GetAccountsQuery>
    {
        public GetAccountsQueryValidator()
        {
            RuleFor(x => x.AccountFilterDto.OwnerId)
                .NotEmpty()
                .When(x => x.AccountFilterDto.OwnerId.HasValue)
                .WithMessage("Owner ID must be a valid GUID");

            RuleFor(x => x.AccountFilterDto.Type)
                .Must(BeValidAccountType)
                .When(x => !string.IsNullOrEmpty(x.AccountFilterDto.Type))
                .WithMessage("Account type must be Deposit, Checking or Credit");

            RuleFor(x => x.AccountFilterDto.Currency)
                .Must(BeValidCurrencyCode)
                .When(x => !string.IsNullOrEmpty(x.AccountFilterDto.Currency))
                .WithMessage("Invalid currency code (ISO 4217)");

            RuleFor(x => x.AccountFilterDto.MinBalance)
                .GreaterThanOrEqualTo(0)
                .When(x => x.AccountFilterDto.MinBalance.HasValue)
                .WithMessage("Minimum balance cannot be negative");

            RuleFor(x => x.AccountFilterDto.MaxBalance)
                .GreaterThanOrEqualTo(0)
                .When(x => x.AccountFilterDto.MaxBalance.HasValue)
                .WithMessage("Maximum balance cannot be negative")
                .GreaterThanOrEqualTo(x => x.AccountFilterDto.MinBalance)
                .When(x => x.AccountFilterDto.MaxBalance.HasValue && x.AccountFilterDto.MinBalance.HasValue)
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
                .When(x => x.AccountFilterDto.MaxInterestRate.HasValue && x.AccountFilterDto.MinInterestRate.HasValue)
                .WithMessage("Maximum interest rate must be greater than or equal to minimum rate");

            RuleFor(x => x.AccountFilterDto.OpeningDateTo)
                .GreaterThanOrEqualTo(x => x.AccountFilterDto.OpeningDateFrom)
                .When(x => x.AccountFilterDto.OpeningDateTo.HasValue && x.AccountFilterDto.OpeningDateFrom.HasValue)
                .WithMessage("Opening date 'to' must be after or equal to 'from' date");

            RuleFor(x => x.AccountFilterDto.ClosingDateTo)
                .GreaterThanOrEqualTo(x => x.AccountFilterDto.ClosingDateFrom)
                .When(x => x.AccountFilterDto.ClosingDateTo.HasValue && x.AccountFilterDto.ClosingDateFrom.HasValue)
                .WithMessage("Closing date 'to' must be after or equal to 'from' date");

            RuleFor(x => x.AccountFilterDto.AccountIds)
                .Must(ids => ids == null || ids.All(id => id != Guid.Empty))
                .When(x => x.AccountFilterDto.AccountIds != null)
                .WithMessage("Account IDs must contain valid GUID values");
        }

        private bool BeValidAccountType(string? type) => type is "Deposit" or "Checking" or "Credit";

        private bool BeValidCurrencyCode(string? currency) => currency is "RUB" or "EUR" or "USD";

        private bool BeValidInterestRate(string? accountType, decimal? rate)
        {
            if (accountType is "Deposit" or "Credit")
            {
                return rate.HasValue && rate > 0 && rate <= 100;
            }
            return !rate.HasValue;
        }
    }
}
