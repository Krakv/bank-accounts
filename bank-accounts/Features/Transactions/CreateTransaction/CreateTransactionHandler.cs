using bank_accounts.Exceptions;
using bank_accounts.Features.Accounts.Entities;
using bank_accounts.Features.Common.UnitOfWork;
using bank_accounts.Features.Outbox;
using bank_accounts.Features.Outbox.Payloads;
using bank_accounts.Features.Transactions.Dto;
using bank_accounts.Features.Transactions.Entities;
using bank_accounts.Infrastructure.Repository;
using MediatR;

namespace bank_accounts.Features.Transactions.CreateTransaction;

public class CreateTransactionHandler(IUnitOfWork unitOfWork, IRepository<Account> accountRepository) : IRequestHandler<CreateTransactionCommand, Guid[]>
{
    public async Task<Guid[]> Handle(CreateTransactionCommand request, CancellationToken cancellationToken)
    {
        var dto = request.CreateTransactionDto;
        var account = await accountRepository.GetByIdAsync(dto.AccountId);
        if (account == null)
        {
            throw new NotFoundAppException("Account", dto.AccountId);
        }
        Account? counterpartyAccount = null;

        if (dto.CounterpartyAccountId.HasValue)
        {
            counterpartyAccount = await accountRepository.GetByIdAsync(dto.CounterpartyAccountId.Value);
            if (counterpartyAccount == null)
            {
                throw new NotFoundAppException("Account", dto.CounterpartyAccountId.Value);
            }
        }

        await unitOfWork.BeginTransactionAsync();

        try
        {
            Transaction[] result;
            if (dto.CounterpartyAccountId.HasValue && counterpartyAccount != null)
            {
                result = await ProcessTransferAsync(dto, account, counterpartyAccount);

                var payload = new TransferCompletedPayload
                {
                    SourceAccountId = result[1].AccountId,
                    DestinationAccountId = result[0].AccountId,
                    Amount = result[0].Value,
                    Currency = result[0].Currency,
                    TransferId = result[1].Id
                };

                var outboxMessage = OutboxMessageFactory.Create(payload.EventId, "MoneyTransferCompleted", payload, payload.Meta.Source, payload.Meta.CorrelationId, payload.Meta.CausationId);

                await unitOfWork.OutboxMessages.CreateAsync(outboxMessage);
                await unitOfWork.OutboxMessages.SaveChangesAsync();
            }
            else
            {
                result = await ProcessSingleTransactionAsync(dto, account);

                switch (result[0].Type)
                {
                    case "Credit":
                    {
                        var payload = new MoneyCreditedPayload
                        {
                            AccountId = result[0].AccountId,
                            Amount = result[0].Value,
                            Currency = result[0].Currency,
                            OperationId = result[0].Id
                        };
                        var outboxMessage = OutboxMessageFactory.Create(payload.EventId, "MoneyCredited", payload,
                            payload.Meta.Source, payload.Meta.CorrelationId, payload.Meta.CausationId);

                        await unitOfWork.OutboxMessages.CreateAsync(outboxMessage);
                        await unitOfWork.OutboxMessages.SaveChangesAsync();
                        break;
                    }

                    case "Debit":
                    {
                        var payload = new MoneyDebitedPayload
                        {
                            AccountId = result[0].AccountId,
                            Amount = result[0].Value,
                            Currency = result[0].Currency,
                            OperationId = result[0].Id,
                            Reason = result[0].Description
                        };
                        var outboxMessage = OutboxMessageFactory.Create(payload.EventId, "MoneyDebited", payload, payload.Meta.Source, payload.Meta.CorrelationId, payload.Meta.CausationId);

                        await unitOfWork.OutboxMessages.CreateAsync(outboxMessage);
                        await unitOfWork.OutboxMessages.SaveChangesAsync();
                        break;
                    }
                }
            }
            await unitOfWork.CommitAsync();
            return result.Select(x => x.Id).ToArray();
        }
        catch (Exception)
        {
            await unitOfWork.RollbackAsync();
            throw;
        }
    }

    private async Task<Transaction[]> ProcessTransferAsync(CreateTransactionDto dto, Account accountDto, Account counterpartyDto)
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

        var accountEntity = (await unitOfWork.Accounts.GetByIdAsync(account.Id))!;
        accountEntity.Balance = account.Balance;
        await unitOfWork.Accounts.Update(accountEntity);

        var counterpartyEntity = (await unitOfWork.Accounts.GetByIdAsync(counterparty.Id))!;
        counterpartyEntity.Balance = counterparty.Balance;
        await unitOfWork.Accounts.Update(counterpartyEntity);

        var totalBefore = accountDto.Balance + counterpartyDto.Balance;
        var totalAfter = account.Balance + counterparty.Balance;

        if (totalBefore == totalAfter) return [receiverTransaction, senderTransaction];
        throw new InvalidOperationException("Failed to create transaction");
    }

    private async Task<Transaction[]> ProcessSingleTransactionAsync(CreateTransactionDto dto, Account accountDto)
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

        var accountEntity = (await unitOfWork.Accounts.GetByIdAsync(account.Id))!;
        accountEntity.Balance = account.Balance;
        await unitOfWork.Accounts.Update(accountEntity);

        return [transaction];
    }
}