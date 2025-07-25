using bank_accounts.Features.Accounts.Dtos;
using bank_accounts.Features.Accounts.Models;

namespace bank_accounts.Features.Accounts
{
    public interface IAccountService
    {
        Task<AccountDto> CreateAccountAsync(PostAccountDto accountDto);
        Task<Account?> GetAccountAsync(Guid id);
        Task<(IEnumerable<Account> accounts, int totalCount)> GetAccountsAsync(AccountFilterDto filter);
        Task<AccountDto> UpdateInterestRateAsync(Guid id, UpdateInterestRateDto updateDto);
        Task<AccountDto> CloseAccountAsync(Guid id);
        Task<AccountStatementResponseDto?> GetAccountStatementAsync(Guid accountId, AccountStatementRequestDto dto);
    }
}
