using bank_accounts.Features.Accounts.Entities;
using bank_accounts.Infrastructure.Repository;
using MediatR;

namespace bank_accounts.Features.Accounts.AccrueInterest;

public class AccrueInterestHandler(IRepository<Account> accountRepository) : IRequestHandler<AccrueInterestCommand>
{
    public async Task Handle(AccrueInterestCommand request, CancellationToken cancellationToken)
    {
        await accountRepository.AccrueInterestAsync(request.AccountId);
    }
}