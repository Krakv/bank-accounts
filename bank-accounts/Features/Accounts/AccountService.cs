using bank_accounts.Features.Accounts.Dtos;
using bank_accounts.Features.Accounts.Models;
using bank_accounts.Features.Transactions.Models;
using bank_accounts.Infrastructure.Repository;

namespace bank_accounts.Features.Accounts
{
    public class AccountService : IAccountService
    {
        private readonly IRepository<Account> _accountRepository;
        private readonly IRepository<Transaction> _transactionRepository;
        private readonly ILogger<AccountService> _logger;

        public AccountService(IRepository<Account> accountRepository, IRepository<Transaction> transactionRepository, ILogger<AccountService> logger)
        {
            _accountRepository = accountRepository;
            _transactionRepository = transactionRepository;
            _logger = logger;
        }

        public async Task<AccountDto> CreateAccountAsync(PostAccountDto accountDto)
        {
            var account = new Account()
            {
                OwnerId = accountDto.OwnerId,
                Type = accountDto.Type,
                Currency = accountDto.Currency,
                InterestRate = accountDto.InterestRate
            };

            await _accountRepository.CreateAsync(account);
            await _accountRepository.SaveChangesAsync();

            return MapToDto(account);
        }

        public async Task<Account?> GetAccountAsync(Guid id)
        {
            return await _accountRepository.GetByIdAsync(id);
        }

        public async Task<(IEnumerable<Account> accounts, int totalCount)> GetAccountsAsync(AccountFilterDto filter)
        {
            if (filter.IsActive.HasValue)
            {
                filter.ClosingDateFrom = filter.IsActive.Value ? null : DateTime.MinValue;
                filter.ClosingDateTo = filter.IsActive.Value ? null : DateTime.MaxValue;
            }

            return await _accountRepository.GetFilteredAsync(filter);
        }

        public async Task<AccountDto> UpdateInterestRateAsync(Guid id, UpdateInterestRateDto updateDto)
        {
            var account = await _accountRepository.GetByIdAsync(id);
            if (account == null)
            {
                throw new KeyNotFoundException($"Account with id {id} not found");
            }

            if (account.Type != "Deposit" && account.Type != "Credit")
            {
                throw new InvalidOperationException("Interest rate can only be updated for Deposit or Credit accounts");
            }

            if (updateDto.InterestRate.HasValue && updateDto.InterestRate <= 0)
            {
                throw new ArgumentException("Interest rate must be greater than 0");
            }

            account.InterestRate = updateDto.InterestRate;
            await _accountRepository.UpdateAsync(account);
            await _accountRepository.SaveChangesAsync();

            return MapToDto(account);
        }

        public async Task<AccountDto> CloseAccountAsync(Guid id)
        {
            var account = await _accountRepository.GetByIdAsync(id);
            if (account == null)
            {
                throw new KeyNotFoundException($"Account with id {id} not found");
            }

            if (account.ClosingDate != null)
            {
                throw new InvalidOperationException($"Account with id {id} is already closed");
            }

            account.ClosingDate = DateTime.UtcNow;
            await _accountRepository.UpdateAsync(account);
            await _accountRepository.SaveChangesAsync();

            return MapToDto(account);
        }

        private static AccountDto MapToDto(Account account)
        {
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

        public async Task<AccountStatementResponseDto?> GetAccountStatementAsync(Guid accountId, AccountStatementRequestDto dto)
        {
            var filter = new StatementFilterDto()
            {
                AccountId = accountId,
                StartDate = dto.StartDate,
                EndDate = dto.EndDate
            };

            var account = await _accountRepository.GetByIdAsync(filter.AccountId);
            if (account == null)
            {
                return null;
            }

            (IEnumerable<Transaction> transactions, _) = await _transactionRepository
                .GetFilteredAsync(filter);

            var closingBalance = account.Balance;

            var openingBalance = closingBalance -
                                  (transactions.Where(t => t.Type == "Debit").Sum(t => t.Value) -
                                  transactions.Where(t => t.Type == "Credit").Sum(t => t.Value));

            var totalCredits = transactions
                .Where(t => t.Type == "Debit")
                .Sum(t => t.Value);

            var totalDebits = transactions
                .Where(t => t.Type == "Credit")
                .Sum(t => t.Value);

            return new AccountStatementResponseDto
            {
                AccountId = account.Id,
                OwnerId = account.OwnerId,
                Currency = account.Currency,
                StartDate = filter.StartDate,
                EndDate = filter.EndDate,
                OpeningBalance = openingBalance,
                ClosingBalance = closingBalance,
                Transactions = transactions.Select(t => new TransactionStatementDto
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
}