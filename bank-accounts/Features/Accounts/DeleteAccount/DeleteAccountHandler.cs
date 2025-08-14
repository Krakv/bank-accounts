using bank_accounts.Exceptions;
using bank_accounts.Features.Accounts.Entities;
using bank_accounts.Infrastructure.Repository;
using MediatR;

namespace bank_accounts.Features.Accounts.DeleteAccount;

public class DeleteAccountHandler(IRepository<Account> accountRepository) : IRequestHandler<DeleteAccountCommand>
{
    public async Task Handle(DeleteAccountCommand request, CancellationToken cancellationToken)
    {
        var account = await accountRepository.GetByIdAsync(request.AccountId);
        if (account == null)
        {
            throw new NotFoundAppException("Account", request.AccountId);
        }
        await accountRepository.DeleteAsync(account);

        await accountRepository.SaveChangesAsync();
    }
}