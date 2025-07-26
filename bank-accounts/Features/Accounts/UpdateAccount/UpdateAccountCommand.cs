using bank_accounts.Features.Accounts.Dtos;
using MediatR;

namespace bank_accounts.Features.Accounts.UpdateAccount
{
    public record UpdateAccountCommand(Guid AccountId, UpdateAccountDto UpdateAccountDto, string AccountType) : IRequest<Guid>;
}
