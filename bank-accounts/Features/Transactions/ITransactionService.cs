using bank_accounts.Features.Transactions.Dtos;

namespace bank_accounts.Features.Transactions
{
    public interface ITransactionService
    {
        Task<TransactionDto> CreateTransactionAsync(CreateTransactionDto dto);
        Task<TransactionDto?> GetTransactionAsync(Guid id);
    }
}
