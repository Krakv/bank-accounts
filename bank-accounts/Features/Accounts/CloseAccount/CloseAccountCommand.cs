using MediatR;

namespace bank_accounts.Features.Accounts.CloseAccount;

public record DeleteAccountCommand(Guid AccountId) : IRequest;