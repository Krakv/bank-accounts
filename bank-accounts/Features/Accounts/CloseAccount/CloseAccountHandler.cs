using bank_accounts.Features.Accounts.Entities;
using bank_accounts.Infrastructure.Repository;
using MediatR;

namespace bank_accounts.Features.Accounts.CloseAccount;

public class CloseAccountHandler(IRepository<Account> accountRepository) : IRequestHandler<CloseAccountCommand>
{
    public async Task Handle(CloseAccountCommand request, CancellationToken cancellationToken)
    {
        await accountRepository.UpdatePartialAsync(
            new Account { Id = request.AccountId, ClosingDate = DateTime.UtcNow },
            x => x.ClosingDate
        );

        await accountRepository.SaveChangesAsync();
    }
}