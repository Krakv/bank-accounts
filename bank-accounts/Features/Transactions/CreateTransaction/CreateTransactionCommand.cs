using bank_accounts.Features.Transactions.Dto;
using MediatR;

namespace bank_accounts.Features.Transactions.CreateTransaction;

public record CreateTransactionCommand(CreateTransactionDto CreateTransactionDto) : IRequest<Guid[]>;