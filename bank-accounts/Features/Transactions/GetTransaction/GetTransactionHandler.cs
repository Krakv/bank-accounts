using bank_accounts.Features.Transactions.Dtos;
using bank_accounts.Features.Transactions.Entities;
using bank_accounts.Infrastructure.Repository;
using MediatR;

namespace bank_accounts.Features.Transactions.GetTransaction
{
    public class GetTransactionHandler(IRepository<Transaction> transactionRepository) : IRequestHandler<GetTransactionQuery, TransactionDto?>
    {
        public async Task<TransactionDto?> Handle(GetTransactionQuery request, CancellationToken cancellationToken)
        {
            var transaction = await transactionRepository.GetByIdAsync(request.Id);
            return transaction == null 
                ? null 
                : new TransactionDto
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
}
