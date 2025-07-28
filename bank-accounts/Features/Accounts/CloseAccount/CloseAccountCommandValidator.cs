using FluentValidation;
using JetBrains.Annotations;

namespace bank_accounts.Features.Accounts.CloseAccount;

[UsedImplicitly]
public class CloseAccountCommandValidator : AbstractValidator<CloseAccountCommand>
{
    public CloseAccountCommandValidator()
    {
        RuleFor(x => x.AccountId)
            .NotEmpty()
            .WithMessage("Account ID is required");
    }
}