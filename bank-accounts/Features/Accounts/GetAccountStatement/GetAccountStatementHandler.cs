using bank_accounts.Features.Accounts.Dto;
using bank_accounts.Features.Accounts.Entities;
using bank_accounts.Features.Transactions.Entities;
using bank_accounts.Infrastructure.Repository;
using MediatR;

namespace bank_accounts.Features.Accounts.GetAccountStatement;

public class GetAccountStatementHandler(IRepository<Account> accountRepository, IRepository<Transaction> transactionRepository) : IRequestHandler<GetAccountStatementQuery, AccountStatementResponseDto?>
{
    public async Task<AccountStatementResponseDto?> Handle(GetAccountStatementQuery request, CancellationToken cancellationToken)
    {
        var accountId = request.AccountId;
        var dto = request.AccountStatementRequestDto;

        var filter = new StatementFilterDto
        {
            AccountId = accountId,
            StartDate = dto.StartDate,
            EndDate = dto.EndDate
        };

        var account = await accountRepository.GetByIdAsync(filter.AccountId);
        if (account == null)
        {
            return null;
        }

        var transactions= (await transactionRepository
            .GetFilteredAsync(filter)).data;

        var enumerable = transactions.ToList();

        var totalCredits = enumerable
            .Where(t => t.Type == "Credit")
            .Sum(t => t.Value);

        var totalDebits = enumerable
            .Where(t => t.Type == "Debit")
            .Sum(t => t.Value);


        return new AccountStatementResponseDto
        {
            AccountId = account.Id,
            OwnerId = account.OwnerId,
            Currency = account.Currency,
            StartDate = filter.StartDate,
            EndDate = filter.EndDate,
            Transactions = enumerable.Select(t => new TransactionStatementDto
            {
                Id = t.Id,
                Type = t.Type,
                Value = t.Value,
                Description = t.Description,
                Date = t.Date,
                CounterpartyAccountId = t.CounterpartyAccountId
            }).ToList(),
            TotalCredits = totalCredits,
            TotalDebits = totalDebits
        };
    }
}