using bank_accounts.Features.Accounts.Dto;
using MediatR;

namespace bank_accounts.Features.Accounts.GetAccountStatementAndExplainAnalyze;

public class GetAccountStatementAndExplainAnalyzeQuery(Guid accountId, AccountStatementRequestDto accountStatementRequestDto) : IRequest<string?>
{
    public Guid AccountId { get; set; } = accountId;
    public AccountStatementRequestDto AccountStatementRequestDto { get; set; } = accountStatementRequestDto;
}