using bank_accounts.Features.Accounts.Models;
using bank_accounts.Features.Transactions.Dtos;
using bank_accounts.Features.Transactions.Models;

namespace bank_accounts.Features.Transactions
{
    public class TransactionService : ITransactionService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<TransactionService> _logger;

        public TransactionService(IUnitOfWork unitOfWork, ILogger<TransactionService> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<TransactionDto> CreateTransactionAsync(CreateTransactionDto dto)
        {
            var account = await _unitOfWork.Accounts.GetByIdAsync(dto.AccountId)
                ?? throw new AccountNotFoundException(dto.AccountId);

            await _unitOfWork.BeginTransactionAsync();

            try
            {
                var result = dto.CounterpartyAccountId.HasValue
                    ? await ProcessTransferAsync(dto, account)
                    : await ProcessSingleTransactionAsync(dto, account);

                await _unitOfWork.CommitAsync();
                return result;
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackAsync();
                _logger.LogError(ex, "Transaction failed");
                throw;
            }
        }

        private async Task<TransactionDto> ProcessTransferAsync(CreateTransactionDto dto, Account account)
        {
            var counterparty = await _unitOfWork.Accounts.GetByIdAsync(dto.CounterpartyAccountId!.Value)
                ?? throw new AccountNotFoundException(dto.CounterpartyAccountId.Value);

            if (account.Currency != counterparty.Currency)
                throw new CurrencyMismatchException(account.Currency.ToString(), counterparty.Currency.ToString());

            if (account.Balance < dto.Value)
                throw new InsufficientFundsException(dto.AccountId);

            var debitTransaction = new Transaction
            {
                Id = Guid.NewGuid(),
                AccountId = dto.AccountId,
                CounterpartyAccountId = dto.CounterpartyAccountId,
                Currency = dto.Currency,
                Value = dto.Value,
                Type = "Credit",
                Description = dto.Description
            };

            var creditTransaction = new Transaction
            {
                Id = Guid.NewGuid(),
                AccountId = dto.CounterpartyAccountId.Value,
                CounterpartyAccountId = dto.AccountId,
                Currency = dto.Currency,
                Value = dto.Value,
                Type = "Debit",
                Description = dto.Description
            };

            account.Balance -= dto.Value;
            counterparty.Balance += dto.Value;

            await _unitOfWork.Transactions.CreateAsync(debitTransaction);
            await _unitOfWork.Transactions.CreateAsync(creditTransaction);
            await _unitOfWork.Accounts.UpdateAsync(account);
            await _unitOfWork.Accounts.UpdateAsync(counterparty);

            return MapToDto(debitTransaction);
        }

        private async Task<TransactionDto> ProcessSingleTransactionAsync(CreateTransactionDto dto, Account account)
        {
            if (account.Currency.ToString() != dto.Currency)
                throw new CurrencyMismatchException(account.Currency.ToString(), dto.Currency);

            if (dto.Type == "Credit" && account.Balance < dto.Value)
                throw new InsufficientFundsException(dto.AccountId);

            var transaction = new Transaction
            {
                Id = Guid.NewGuid(),
                AccountId = dto.AccountId,
                CounterpartyAccountId = null,
                Currency = dto.Currency,
                Value = dto.Value,
                Type = dto.Type,
                Description = dto.Description
            };

            account.Balance = dto.Type == "Debit"
                ? account.Balance + dto.Value
                : account.Balance - dto.Value;

            await _unitOfWork.Transactions.CreateAsync(transaction);
            await _unitOfWork.Accounts.UpdateAsync(account);

            return MapToDto(transaction);
        }

        private TransactionDto MapToDto(Transaction transaction) => new()
        {
            TransactionId = transaction.Id,
            AccountId = transaction.AccountId,
            CounterpartyAccountId = transaction.CounterpartyAccountId,
            Currency = transaction.Currency,
            Value = transaction.Value,
            Type = transaction.Type.ToString(),
            Description = transaction.Description,
            Date = transaction.Date
        };

        public async Task<TransactionDto?> GetTransactionAsync(Guid id)
        {
            var transaction = await _unitOfWork.Transactions.GetByIdAsync(id);
            return transaction != null ? new TransactionDto
            {
                TransactionId = transaction.Id,
                AccountId = transaction.AccountId,
                CounterpartyAccountId = transaction.CounterpartyAccountId,
                Currency = transaction.Currency,
                Value = transaction.Value,
                Type = transaction.Type.ToString(),
                Description = transaction.Description,
                Date = transaction.Date
            } : null;
        }
    }
}
