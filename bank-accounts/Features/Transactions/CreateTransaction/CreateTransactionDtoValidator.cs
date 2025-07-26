using bank_accounts.Features.Transactions.Dtos;
using FluentValidation;

namespace bank_accounts.Features.Transactions.CreateTransaction
{
    public class CreateTransactionDtoValidator : AbstractValidator<CreateTransactionDto>
    {
        public CreateTransactionDtoValidator()
        {
            RuleFor(x => x.AccountId)
                .NotEmpty()
                .WithMessage("Account ID is required");

            RuleFor(x => x.Currency)
                .NotEmpty()
                .Length(3)
                .WithMessage("Currency must be 3-letter code")
                .Must(BeValidCurrency)
                .WithMessage("Invalid currency code");

            RuleFor(x => x.Value)
                .GreaterThan(0)
                .WithMessage("Value must be greater than 0");

            RuleFor(x => x.Type)
                .NotEmpty()
                .Must(BeValidTransactionType)
                .WithMessage("Type must be either 'Credit' or 'Debit'");

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

        private bool BeValidCurrency(string currency)
        {
            return currency is "RUB" or "USD" or "EUR";
        }

        private bool BeValidTransactionType(string type)
        {
            return type is "Credit" or "Debit";
        }
    }
}
