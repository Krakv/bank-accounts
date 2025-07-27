using bank_accounts.Features.Accounts.Dto;
using bank_accounts.Features.Transactions.Dto;
using MediatR;

namespace bank_accounts.Features.Transactions.CreateTransaction;

public record CreateTransactionCommand(CreateTransactionDto CreateTransactionDto, AccountDto Account, AccountDto? CounterpartyAccount) : IRequest<Guid?>;