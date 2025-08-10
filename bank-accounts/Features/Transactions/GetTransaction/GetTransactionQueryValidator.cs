using bank_accounts.Features.Transactions.Entities;
using bank_accounts.Infrastructure.Repository;
using FluentValidation;
using JetBrains.Annotations;

namespace bank_accounts.Features.Transactions.GetTransaction;

[UsedImplicitly]
public class GetTransactionQueryValidator : AbstractValidator<GetTransactionQuery>
{
    private readonly IRepository<Transaction> _transactionRepository;

    public GetTransactionQueryValidator(IRepository<Transaction> transactionRepository)
    {
        _transactionRepository = transactionRepository;

        RuleFor(x => x.Id)
            .MustAsync(TransactionExist)
            .WithMessage("Account not found")
            .WithErrorCode("404");
    }

    private async Task<bool> TransactionExist(Guid transactionId, CancellationToken cancellationToken)
    {
        return await _transactionRepository.GetByIdAsync(transactionId) != null;
    }
}