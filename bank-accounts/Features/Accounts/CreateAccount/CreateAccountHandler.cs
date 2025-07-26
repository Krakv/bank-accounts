using bank_accounts.Features.Accounts.Entities;
using bank_accounts.Infrastructure.Repository;
using MediatR;

namespace bank_accounts.Features.Accounts.CreateAccount
{
    public class CreateAccountHandler(IRepository<Account> accountRepository) : IRequestHandler<CreateAccountCommand, Guid>
    {
        public async Task<Guid> Handle(CreateAccountCommand request, CancellationToken cancellationToken)
        {
            var accountDto = request.CreateAccountDto;
            var account = new Account()
            {
                OwnerId = accountDto.OwnerId,
                Type = accountDto.Type,
                Currency = accountDto.Currency,
                InterestRate = accountDto.InterestRate
            };

            await accountRepository.CreateAsync(account);
            await accountRepository.SaveChangesAsync();

            return account.Id;
        }
    }
}
