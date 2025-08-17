using bank_accounts.Features.Inbox.Payloads;
using MediatR;

namespace bank_accounts.Features.Accounts.FrozeClientAccounts;

public record FrozeClientAccountsCommand(Guid ClientId, ClientBlockingPayload? EventPayload = null) : IRequest;