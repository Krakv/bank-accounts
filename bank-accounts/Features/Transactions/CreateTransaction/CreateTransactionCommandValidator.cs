using FluentValidation;

namespace bank_accounts.Features.Transactions.CreateTransaction
{
    public class CreateTransactionCommandValidator : AbstractValidator<CreateTransactionCommand>
    {
        public CreateTransactionCommandValidator()
        {
            RuleFor(x => x.CreateTransactionDto)
                .NotNull()
                .SetValidator(new CreateTransactionDtoValidator());

            RuleFor(x => x.Account)
                .NotNull()
                .WithMessage("Account not found");

            When(x => x.CreateTransactionDto.CounterpartyAccountId.HasValue, () =>
            {
                RuleFor(x => x.CounterpartyAccount)
                    .NotNull()
                    .WithMessage("Counterparty account not found");

                RuleFor(x => x.CreateTransactionDto.Currency)
                    .Equal(x => x.CounterpartyAccount!.Currency)
                    .When(x => x.CounterpartyAccount != null)
                    .WithMessage("Currency must match between accounts");

                When(x => 
                    x.CreateTransactionDto.Type == "Credit" ||
                    x.CreateTransactionDto.Type == "Debit", () =>
                {
                    RuleFor(x => x.CreateTransactionDto.Value)
                        .Must((command, value) =>
                            command.CreateTransactionDto.Type == "Credit" ||
                            command.Account.Balance >= value)
                        .WithMessage("Insufficient funds for debit transaction");
                });


            });
        }
    }
}
