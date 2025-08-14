using bank_accounts.Exceptions;
using bank_accounts.Features.Accounts.Dto;
using bank_accounts.Features.Accounts.Entities;
using bank_accounts.Infrastructure.Repository;
using MediatR;

namespace bank_accounts.Features.Accounts.GetAccount;

public class GetAccountHandler(IRepository<Account> accountRepository) : IRequestHandler<GetAccountQuery, AccountDto>
{
    public async Task<AccountDto> Handle(GetAccountQuery request, CancellationToken cancellationToken)
    {
        var account = await accountRepository.GetByIdAsync(request.Id);

        if (account == null)
        {
            throw new NotFoundAppException("Account", request.Id);
        }

        return new AccountDto
            {
                Id = account.Id,
                OwnerId = account.OwnerId,
                Type = account.Type,
                Currency = account.Currency,
                Balance = account.Balance,
                InterestRate = account.InterestRate,
                OpeningDate = account.OpeningDate,
                ClosingDate = account.ClosingDate
            };
    }
}