using FluentValidation;
using JetBrains.Annotations;

namespace bank_accounts.Features.Accounts.CloseAccount;

[UsedImplicitly]
public class DeleteAccountCommandValidator : AbstractValidator<DeleteAccountCommand>
{
    public DeleteAccountCommandValidator()
    {
        RuleFor(x => x.AccountId)
            .NotEmpty()
            .WithMessage("Account ID is required");
    }
}