using bank_accounts.Features.Accounts.Dto;
using MediatR;

namespace bank_accounts.Features.Accounts.CreateAccount
{
    public record CreateAccountCommand(CreateAccountDto CreateAccountDto) : IRequest<Guid>;
}
