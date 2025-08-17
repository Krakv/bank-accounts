using bank_accounts.Features.Inbox.Payloads;
using MediatR;

namespace bank_accounts.Features.Accounts.UnfrozeClientAccounts;

public record UnfrozeClientAccountsCommand(Guid ClientId, ClientBlockingPayload? EventPayload = null) : IRequest<bool>;