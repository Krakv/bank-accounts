using bank_accounts.Features.Accounts.Dtos;
using bank_accounts.Features.Accounts.Entities;
using bank_accounts.Infrastructure.Repository;
using MediatR;

namespace bank_accounts.Features.Accounts.GetAccounts
{
    public class GetAccountsHandler(IRepository<Account> accountRepository) : IRequestHandler<GetAccountsQuery, AccountsDto>
    {
        public async Task<AccountsDto> Handle(GetAccountsQuery request, CancellationToken cancellationToken)
        {
            var filter = request.AccountFilterDto;

            if (filter.IsActive.HasValue)
            {
                filter.ClosingDateFrom = filter.IsActive.Value ? null : DateTime.MinValue;
                filter.ClosingDateTo = filter.IsActive.Value ? null : DateTime.MaxValue;
            }

            var (accounts, totalCount) = await accountRepository.GetFilteredAsync(filter);

            var accountsDto = accounts == null
                ? null
                : accounts.Select(account => new AccountDto
                (
                    Id: account.Id,
                    OwnerId: account.OwnerId,
                    Type: account.Type,
                    Currency: account.Currency,
                    Balance: account.Balance,
                    InterestRate: account.InterestRate,
                    OpeningDate: account.OpeningDate,
                    ClosingDate: account.ClosingDate
                ));

            var result = new AccountsDto()
            {
                Accounts = accountsDto ?? null,
                Pagination = new PaginationDto()
                {
                    Page = filter.Page,
                    PageSize = filter.PageSize,
                    TotalCount = totalCount,
                    TotalPages = (int)Math.Ceiling((double)totalCount / filter.PageSize)
                }
            };

            return result;
        }
    }
}
