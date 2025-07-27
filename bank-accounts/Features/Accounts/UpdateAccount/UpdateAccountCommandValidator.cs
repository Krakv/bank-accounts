using FluentValidation;
using JetBrains.Annotations;

namespace bank_accounts.Features.Accounts.UpdateAccount;

[UsedImplicitly]
public class UpdateAccountCommandValidator : AbstractValidator<UpdateAccountCommand>
{
    public UpdateAccountCommandValidator()
    {
        When(x => x.AccountType is "Deposit" or "Credit", () =>
        {
            RuleFor(x => x.UpdateAccountDto.InterestRate)
                .NotNull()
                .WithMessage("Interest rate is required for Deposit/Credit accounts")
                .GreaterThanOrEqualTo(0)
                .WithMessage("Interest rate must be greater than or equal to 0 for Deposit/Credit accounts")
                .LessThanOrEqualTo(100)
                .WithMessage("Interest rate cannot exceed 100 for Deposit/Credit accounts");
        });

        When(x => x.AccountType == "Checking", () =>
        {
            RuleFor(x => x.UpdateAccountDto.InterestRate)
                .Null()
                .WithMessage("Interest rate must be null for Checking accounts");
        });
    }
}