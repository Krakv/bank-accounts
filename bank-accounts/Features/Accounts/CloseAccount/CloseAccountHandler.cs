using bank_accounts.Features.Accounts.Entities;
using bank_accounts.Infrastructure.Repository;
using MediatR;

namespace bank_accounts.Features.Accounts.CloseAccount;

public class CloseAccountHandler(IRepository<Account> accountRepository) : IRequestHandler<CloseAccountCommand>
{
    public async Task Handle(CloseAccountCommand request, CancellationToken cancellationToken)
    {
        var account = (await accountRepository.GetByIdAsync(request.AccountId))!;
        account.ClosingDate = DateTime.UtcNow;
        await accountRepository.Update(account);

        await accountRepository.SaveChangesAsync();
    }
}