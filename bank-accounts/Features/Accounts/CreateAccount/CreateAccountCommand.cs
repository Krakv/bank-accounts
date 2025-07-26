using bank_accounts.Features.Accounts.Dtos;
using MediatR;

namespace bank_accounts.Features.Accounts.CreateAccount
{
    public record CreateAccountCommand(CreateAccountDto CreateAccountDto) : IRequest<Guid>;
}
