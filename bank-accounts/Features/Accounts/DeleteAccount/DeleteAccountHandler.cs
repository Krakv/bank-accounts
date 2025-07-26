using bank_accounts.Features.Accounts.Entities;
using bank_accounts.Infrastructure.Repository;
using MediatR;

namespace bank_accounts.Features.Accounts.DeleteAccount
{
    public class DeleteAccountHandler(IRepository<Account> accountRepository) : IRequestHandler<DeleteAccountCommand>
    {
        public async Task Handle(DeleteAccountCommand request, CancellationToken cancellationToken)
        {
            await accountRepository.UpdatePartialAsync(
                new Account { Id = request.AccountId, ClosingDate = DateTime.UtcNow },
                x => x.ClosingDate
            );

            await accountRepository.SaveChangesAsync();
        }
    }
}
