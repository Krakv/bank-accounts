using bank_accounts.Exceptions;
using bank_accounts.Features.Transactions.Dto;
using bank_accounts.Features.Transactions.Entities;
using bank_accounts.Infrastructure.Repository;
using MediatR;

namespace bank_accounts.Features.Transactions.GetTransaction;

public class GetTransactionHandler(IRepository<Transaction> transactionRepository) : IRequestHandler<GetTransactionQuery, TransactionDto?>
{
    public async Task<TransactionDto?> Handle(GetTransactionQuery request, CancellationToken cancellationToken)
    {
        var transaction = await transactionRepository.GetByIdAsync(request.Id);
        if (transaction == null)
        {
            throw new NotFoundAppException("Transaction", request.Id);
        }
        return new TransactionDto
            {
                TransactionId = transaction.Id,
                AccountId = transaction.AccountId,
                CounterpartyAccountId = transaction.CounterpartyAccountId,
                Currency = transaction.Currency,
                Value = transaction.Value,
                Type = transaction.Type,
                Description = transaction.Description,
                Date = transaction.Date
            };
    }
}