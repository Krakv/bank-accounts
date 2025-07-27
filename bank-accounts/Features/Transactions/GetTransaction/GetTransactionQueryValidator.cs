using FluentValidation;

namespace bank_accounts.Features.Transactions.GetTransaction
{
    public class GetTransactionQueryValidator : AbstractValidator<GetTransactionQuery>
    {
        public GetTransactionQueryValidator()
        {
            RuleFor(x => x.Id)
                .NotEmpty()
                .WithMessage("Transaction ID is required");
        }
    }
}
