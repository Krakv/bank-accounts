using FluentValidation;

namespace bank_accounts.Features.Accounts.DeleteAccount
{
    public class DeleteAccountCommandValidator : AbstractValidator<DeleteAccountCommand>
    {
        public DeleteAccountCommandValidator()
        {
            RuleFor(x => x.AccountId)
                .NotEmpty()
                .WithMessage("Account ID is required");
        }
    }
}
