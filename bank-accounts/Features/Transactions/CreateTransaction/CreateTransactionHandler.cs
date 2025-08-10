using bank_accounts.Features.Accounts.Entities;
using bank_accounts.Features.Transactions.Dto;
using bank_accounts.Features.Transactions.Entities;
using bank_accounts.Infrastructure.Repository;
using MediatR;

namespace bank_accounts.Features.Transactions.CreateTransaction;

public class CreateTransactionHandler(IUnitOfWork unitOfWork, IRepository<Account> accountRepository) : IRequestHandler<CreateTransactionCommand, Guid[]?>
{
    public async Task<Guid[]?> Handle(CreateTransactionCommand request, CancellationToken cancellationToken)
    {
        var dto = request.CreateTransactionDto;
        var account = (await accountRepository.GetByIdAsync(dto.AccountId))!;
        Account? counterpartyAccount = null;

        if (dto.CounterpartyAccountId.HasValue)
            counterpartyAccount = await accountRepository.GetByIdAsync(dto.CounterpartyAccountId.Value);

        await unitOfWork.BeginTransactionAsync();

        try
        {
            Guid[]? result;
            if (dto.CounterpartyAccountId.HasValue)
            {
                if (counterpartyAccount != null)
                    result = await ProcessTransferAsync(dto, account, counterpartyAccount);
                else
                    result = null;
            }
            else
                result = await ProcessSingleTransactionAsync(dto, account);

            await unitOfWork.CommitAsync();
            return result;
        }
        catch (Exception)
        {
            await unitOfWork.RollbackAsync();
            throw;
        }
    }

    private async Task<Guid[]?> ProcessTransferAsync(CreateTransactionDto dto, Account accountDto, Account counterpartyDto)
    {
        Account account = new() { Id = accountDto.Id, Balance = accountDto.Balance };
        Account counterparty = new() { Id = counterpartyDto.Id, Balance = counterpartyDto.Balance };

        Transaction senderTransaction;
        Transaction receiverTransaction;

        switch (dto.Type)
        {
            case "Debit":
                senderTransaction = new Transaction
                {
                    Id = Guid.NewGuid(),
                    AccountId = accountDto.Id,
                    CounterpartyAccountId = counterpartyDto.Id,
                    Currency = dto.Currency,
                    Value = dto.Value,
                    Type = "Debit",
                    Description = dto.Description,
                    Date = DateTime.UtcNow
                };

                receiverTransaction = new Transaction
                {
                    Id = Guid.NewGuid(),
                    AccountId = counterpartyDto.Id,
                    CounterpartyAccountId = accountDto.Id,
                    Currency = dto.Currency,
                    Value = dto.Value,
                    Type = "Credit",
                    Description = dto.Description,
                    Date = DateTime.UtcNow
                };

                account.Balance -= dto.Value;
                counterparty.Balance += dto.Value;
                break;
            case "Credit":
                senderTransaction = new Transaction
                {
                    Id = Guid.NewGuid(),
                    AccountId = counterpartyDto.Id,
                    CounterpartyAccountId = accountDto.Id,
                    Currency = dto.Currency,
                    Value = dto.Value,
                    Type = "Debit",
                    Description = dto.Description,
                    Date = DateTime.UtcNow
                };

                receiverTransaction = new Transaction
                {
                    Id = Guid.NewGuid(),
                    AccountId = accountDto.Id,
                    CounterpartyAccountId = counterpartyDto.Id,
                    Currency = dto.Currency,
                    Value = dto.Value,
                    Type = "Credit",
                    Description = dto.Description,
                    Date = DateTime.UtcNow
                };

                counterparty.Balance -= dto.Value;
                account.Balance += dto.Value;
                break;
            default:
                throw new ArgumentException($"Unknown transaction type: {dto.Type}");
        }
            
        await unitOfWork.Transactions.CreateAsync(senderTransaction);
        await unitOfWork.Transactions.CreateAsync(receiverTransaction);
        await unitOfWork.Accounts.UpdatePartialAsync(account, x => x.Balance);
        await unitOfWork.Accounts.UpdatePartialAsync(counterparty, x => x.Balance);

        var totalBefore = accountDto.Balance + counterpartyDto.Balance;
        var totalAfter = account.Balance + counterparty.Balance;

        if (totalBefore == totalAfter) return [receiverTransaction.Id, senderTransaction.Id];
        await unitOfWork.RollbackAsync();
        return null;

    }

    private async Task<Guid[]> ProcessSingleTransactionAsync(CreateTransactionDto dto, Account accountDto)
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

        return [transaction.Id];
    }
}