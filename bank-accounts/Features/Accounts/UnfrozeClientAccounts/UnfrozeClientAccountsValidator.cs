using FluentValidation;
using JetBrains.Annotations;

namespace bank_accounts.Features.Accounts.UnfrozeClientAccounts;

[UsedImplicitly]
public class UnfrozeClientAccountsValidator : AbstractValidator<UnfrozeClientAccountsCommand>
{
    public UnfrozeClientAccountsValidator()
    {
        RuleFor(x => x.ClientId)
            .NotEmpty()
            .WithMessage("ClientId must be valid.");

        When(x => x.EventPayload != null, () =>
        {
            RuleFor(x => x.EventPayload!.Meta)
                .Must(meta => !string.IsNullOrWhiteSpace(meta.Version))
                .WithMessage("Meta.Version is required.")
                .Must(meta => IsVersionSupported(meta.Version))
                .WithMessage("Unsupported message version.");

            RuleFor(x => x.EventPayload!.EventId)
                .NotEmpty()
                .WithMessage("EventId is required.");

            RuleFor(x => x.EventPayload!.OccuredAt)
                .NotEmpty()
                .WithMessage("OccuredAt is required.");

            RuleFor(x => x.EventPayload!.Meta.Source)
                .NotEmpty()
                .WithMessage("Meta.Source is required.");

            RuleFor(x => x.EventPayload!.Meta.CorrelationId)
                .NotEmpty()
                .WithMessage("Meta.CorrelationId is required.");

            RuleFor(x => x.EventPayload!.Meta.CausationId)
                .NotEmpty()
                .WithMessage("Meta.CausationId is required.");
        });
    }

    private static bool IsVersionSupported(string version)
    {
        return version == "v1";
    }
}