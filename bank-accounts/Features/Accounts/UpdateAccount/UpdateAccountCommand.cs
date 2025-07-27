using bank_accounts.Features.Accounts.Dto;
using MediatR;

namespace bank_accounts.Features.Accounts.UpdateAccount;

public record UpdateAccountCommand(Guid AccountId, UpdateAccountDto UpdateAccountDto, string AccountType) : IRequest<Guid>;