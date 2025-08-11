using bank_accounts.Exceptions;
using bank_accounts.Features.Accounts.Entities;
using bank_accounts.Infrastructure.Repository;
using MediatR;

namespace bank_accounts.Features.Accounts.UpdateAccount;

public class UpdateAccountHandler(IRepository<Account> accountRepository) : IRequestHandler<UpdateAccountCommand, Guid>
{
    public async Task<Guid> Handle(UpdateAccountCommand request, CancellationToken cancellationToken)
    {
        var account = await accountRepository.GetByIdAsync(request.AccountId);
        if (account == null)
        {
            throw new NotFoundAppException("Account", request.AccountId);
        }
        account.InterestRate = request.UpdateAccountDto.InterestRate;
        await accountRepository.Update(account);

        await accountRepository.SaveChangesAsync();

        return request.AccountId;
    }
}