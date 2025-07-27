using bank_accounts.Features.Accounts.Dto;
using MediatR;

namespace bank_accounts.Features.Accounts.GetAccountStatement;

public class GetAccountStatementQuery(Guid accountId, AccountStatementRequestDto accountStatementRequestDto) : IRequest<AccountStatementResponseDto?>
{
    public Guid AccountId { get; set; } = accountId;
    public AccountStatementRequestDto AccountStatementRequestDto { get; set; } = accountStatementRequestDto;
}