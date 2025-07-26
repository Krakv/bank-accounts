using bank_accounts.Features.Accounts.Dtos;
using bank_accounts.Features.Accounts.Entities;
using bank_accounts.Features.Transactions.Dtos;
using bank_accounts.Features.Transactions.Entities;
using bank_accounts.Infrastructure.Repository;
using MediatR;

namespace bank_accounts.Features.Transactions.CreateTransaction
{
    public class CreateTransactionHandler(IRepository<Transaction> accountRepository, IUnitOfWork unitOfWork) : IRequestHandler<CreateTransactionCommand, Guid>
    {
        public async Task<Guid> Handle(CreateTransactionCommand request, CancellationToken cancellationToken)
        {
            var dto = request.CreateTransactionDto;
            var account = request.Account;
            var counterpartyAccount = request.CounterpartyAccount;

            await unitOfWork.BeginTransactionAsync();

            try
            {
                var result = dto.CounterpartyAccountId.HasValue
                    ? await ProcessTransferAsync(dto, account, counterpartyAccount)
                    : await ProcessSingleTransactionAsync(dto, account);

                await unitOfWork.CommitAsync();
                return result;
            }
            catch (Exception ex)
            {
                await unitOfWork.RollbackAsync();
                throw;
            }
        }

        private async Task<Guid> ProcessTransferAsync(CreateTransactionDto dto, AccountDto accountDto, AccountDto counterpartyDto)
        {
            Account account = new() { Id = accountDto.Id, Balance = accountDto.Balance };
            Account counterparty = new() { Id = counterpartyDto.Id, Balance = counterpartyDto.Balance };

            Transaction senderTransaction;
            Transaction receiverTransaction;

            if (dto.Type == "Debit")
            {
                senderTransaction = new Transaction
                {
                    Id = Guid.NewGuid(),
                    AccountId = dto.AccountId,
                    CounterpartyAccountId = dto.CounterpartyAccountId,
                    Currency = dto.Currency,
                    Value = dto.Value,
                    Type = "Debit",
                    Description = dto.Description,
                    Date = DateTime.UtcNow
                };

                receiverTransaction = new Transaction
                {
                    Id = Guid.NewGuid(),
                    AccountId = dto.CounterpartyAccountId.Value,
                    CounterpartyAccountId = dto.AccountId,
                    Currency = dto.Currency,
                    Value = dto.Value,
                    Type = "Credit",
                    Description = dto.Description,
                    Date = DateTime.UtcNow
                };

                account.Balance -= dto.Value;
                counterparty.Balance += dto.Value;
            }
            else if (dto.Type == "Credit")
            {
                senderTransaction = new Transaction
                {
                    Id = Guid.NewGuid(),
                    AccountId = dto.CounterpartyAccountId.Value,
                    CounterpartyAccountId = dto.AccountId,
                    Currency = dto.Currency,
                    Value = dto.Value,
                    Type = "Debit",
                    Description = dto.Description,
                    Date = DateTime.UtcNow
                };

                receiverTransaction = new Transaction
                {
                    Id = Guid.NewGuid(),
                    AccountId = dto.AccountId,
                    CounterpartyAccountId = dto.CounterpartyAccountId,
                    Currency = dto.Currency,
                    Value = dto.Value,
                    Type = "Credit",
                    Description = dto.Description,
                    Date = DateTime.UtcNow
                };

                counterparty.Balance -= dto.Value;
                account.Balance += dto.Value;
            }
            else
            {
                throw new ArgumentException($"Unknown transaction type: {dto.Type}");
            }
            
            await unitOfWork.Transactions.CreateAsync(senderTransaction);
            await unitOfWork.Transactions.CreateAsync(receiverTransaction);
            await unitOfWork.Accounts.UpdatePartialAsync(account, x => x.Balance);
            await unitOfWork.Accounts.UpdatePartialAsync(counterparty, x => x.Balance);

            return receiverTransaction.AccountId == account.Id
                ? receiverTransaction.Id
                : senderTransaction.Id;
        }

        private async Task<Guid> ProcessSingleTransactionAsync(CreateTransactionDto dto, AccountDto accountDto)
        {
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

            Account account = new() { Id = accountDto.Id, Balance = accountDto.Balance };

            account.Balance = dto.Type == "Debit"
                ? account.Balance - dto.Value
                : account.Balance + dto.Value;

            await unitOfWork.Transactions.CreateAsync(transaction);
            await unitOfWork.Accounts.UpdatePartialAsync(account, x => x.Balance);

            return transaction.Id;
        }
    }
}
