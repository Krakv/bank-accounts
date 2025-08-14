using bank_accounts.Features.Accounts.Entities;
using bank_accounts.Infrastructure.Repository;
using FluentValidation;
using JetBrains.Annotations;

namespace bank_accounts.Features.Accounts.AccrueInterest;

[UsedImplicitly]
public class AccrueInterestCommandValidator : AbstractValidator<AccrueInterestCommand>
{
    private readonly IRepository<Account> _accountRepository;

    public AccrueInterestCommandValidator(IRepository<Account> accountRepository)
    {
        _accountRepository = accountRepository;

        RuleFor(x => x.AccountId)
            .NotEmpty()
            .WithMessage("Account ID is required");

        RuleFor(x => x.AccountId)
            .NotEmpty()
            .MustAsync(IsDeposit)
            .WithMessage("Account must be opened deposit with positive InterestRate");
    }

    private async Task<bool> IsDeposit(Guid accountId, CancellationToken cancellationToken)
    {
        var account = await _accountRepository.GetByIdAsync(accountId);
        return account is { Type: "Deposit", InterestRate: > 0, ClosingDate: null };
    }
}