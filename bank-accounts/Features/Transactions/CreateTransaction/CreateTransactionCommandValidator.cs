using bank_accounts.Exceptions;
using bank_accounts.Features.Accounts.Entities;
using bank_accounts.Infrastructure.Repository;
using bank_accounts.Services.CurrencyService;
using FluentValidation;
using JetBrains.Annotations;

namespace bank_accounts.Features.Transactions.CreateTransaction;

[UsedImplicitly]
public class CreateTransactionCommandValidator : AbstractValidator<CreateTransactionCommand>
{
    private readonly IRepository<Account> _accountRepository;

    public CreateTransactionCommandValidator(ICurrencyService currencyService, IRepository<Account> accountRepository)
    {
        _accountRepository = accountRepository;

        RuleFor(x => x.CreateTransactionDto)
            .NotNull()
            .SetValidator(new CreateTransactionDtoValidator(currencyService));

        RuleFor(x => x.CreateTransactionDto.AccountId)
            .NotEmpty()
            .WithMessage("Account Id is required");

        When(x => x.CreateTransactionDto.CounterpartyAccountId.HasValue, () =>
        {
            RuleFor(x => x.CreateTransactionDto.Currency)
                .MustAsync(CurrencyCheck)
                .WithMessage("Currency must match between accounts");

            When(x => x.CreateTransactionDto.Type is "Credit" or "Debit", () =>
            {
                RuleFor(x => x.CreateTransactionDto.Value)
                    .MustAsync(BalanceCheck)
                    .WithMessage("Insufficient funds for debit transaction");
            });
        });
    }

    private async Task<bool> BalanceCheck(CreateTransactionCommand command, decimal value, CancellationToken cancellation)
    {
        if (command.CreateTransactionDto.Type == "Credit") return true;

        var account = await _accountRepository.GetByIdAsync(command.CreateTransactionDto.AccountId);
        if (account == null)
        {
            throw new NotFoundAppException("Account", command.CreateTransactionDto.AccountId);
        }
        return account.Balance >= value;
    }

    private async Task<bool> CurrencyCheck(CreateTransactionCommand command, string currency, CancellationToken cancellation)
    {
        var counterparty = await _accountRepository.GetByIdAsync(command.CreateTransactionDto.CounterpartyAccountId!.Value);
        if (counterparty == null)
        {
            throw new NotFoundAppException("Account", command.CreateTransactionDto.CounterpartyAccountId!.Value);
        }
        return counterparty.Currency == currency;
    }
}