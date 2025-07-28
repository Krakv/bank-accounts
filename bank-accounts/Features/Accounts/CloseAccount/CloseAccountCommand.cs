using MediatR;

namespace bank_accounts.Features.Accounts.CloseAccount;

public record CloseAccountCommand(Guid AccountId) : IRequest;