using bank_accounts.Features.Accounts.Dtos;
using bank_accounts.Features.Transactions.Dtos;
using MediatR;

namespace bank_accounts.Features.Transactions.CreateTransaction
{
    public record CreateTransactionCommand(CreateTransactionDto CreateTransactionDto, AccountDto Account, AccountDto? CounterpartyAccount) : IRequest<Guid>;
}
