using bank_accounts.Features.Accounts.Entities;
using bank_accounts.Infrastructure.Repository;
using MediatR;

namespace bank_accounts.Features.Accounts.UpdateAccount
{
    public class UpdateAccountHandler(IRepository<Account> accountRepository) : IRequestHandler<UpdateAccountCommand, Guid>
    {
        public async Task<Guid> Handle(UpdateAccountCommand request, CancellationToken cancellationToken)
        {
            await accountRepository.UpdatePartialAsync(
                new Account { Id = request.AccountId, InterestRate = request.UpdateAccountDto.InterestRate },
                x => x.InterestRate
            );

            await accountRepository.SaveChangesAsync();

            return request.AccountId;
        }
    }
}
