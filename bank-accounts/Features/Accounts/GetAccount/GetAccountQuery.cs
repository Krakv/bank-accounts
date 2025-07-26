using bank_accounts.Features.Accounts.Dtos;
using MediatR;

namespace bank_accounts.Features.Accounts.GetAccount
{
    public class GetAccountQuery(Guid id) : IRequest<AccountDto?>
    {
        public Guid Id { get; set; } = id;
    }
}
