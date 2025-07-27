using FluentValidation;
using JetBrains.Annotations;

namespace bank_accounts.Features.Accounts.GetAccount;

[UsedImplicitly]
public class DeleteAccountCommandValidator : AbstractValidator<GetAccountQuery>
{
    public DeleteAccountCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty()
            .WithMessage("Account ID is required");
    }
}