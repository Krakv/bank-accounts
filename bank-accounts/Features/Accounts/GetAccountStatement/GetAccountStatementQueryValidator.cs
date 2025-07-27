using FluentValidation;
using JetBrains.Annotations;

namespace bank_accounts.Features.Accounts.GetAccountStatement;

[UsedImplicitly]
public class GetAccountQueryValidator : AbstractValidator<GetAccountStatementQuery>
{
    public GetAccountQueryValidator()
    {
        RuleFor(x => x.AccountId)
            .NotEmpty()
            .WithMessage("Account ID is required");

        RuleFor(x => x.AccountStatementRequestDto)
            .NotNull()
            .WithMessage("Request parameters are required");

        RuleFor(x => x.AccountStatementRequestDto.StartDate)
            .NotEmpty()
            .WithMessage("Start date is required")
            .LessThanOrEqualTo(x => x.AccountStatementRequestDto.EndDate)
            .WithMessage("Start date must be before or equal to end date");

        RuleFor(x => x.AccountStatementRequestDto.EndDate)
            .NotEmpty()
            .WithMessage("End date is required")
            .GreaterThanOrEqualTo(x => x.AccountStatementRequestDto.StartDate)
            .WithMessage("End date must be after or equal to start date")
            .LessThanOrEqualTo(DateTime.Today.AddDays(1))
            .WithMessage("End date cannot be in the future");
    }
}