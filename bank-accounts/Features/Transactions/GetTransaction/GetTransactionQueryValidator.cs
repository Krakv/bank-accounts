using FluentValidation;
using JetBrains.Annotations;

namespace bank_accounts.Features.Transactions.GetTransaction;

[UsedImplicitly]
public class GetTransactionQueryValidator : AbstractValidator<GetTransactionQuery>
{
    public GetTransactionQueryValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty()
            .WithMessage("Transaction ID is required");
    }
}