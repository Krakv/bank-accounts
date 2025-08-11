using MediatR;

namespace bank_accounts.Features.Accounts.AccrueInterest;

public record AccrueInterestCommand(Guid AccountId) : IRequest;