using FluentValidation;

namespace bank_accounts.Features.Accounts.GetAccount
{
    public class DeleteAccountCommandValidator : AbstractValidator<GetAccountQuery>
    {
        public DeleteAccountCommandValidator()
        {
            RuleFor(x => x.Id)
                .NotEmpty()
                .WithMessage("Account ID is required");
        }
    }
}
