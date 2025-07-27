using MediatR;

namespace bank_accounts.Features.Accounts.DeleteAccount;

public record DeleteAccountCommand(Guid AccountId) : IRequest;