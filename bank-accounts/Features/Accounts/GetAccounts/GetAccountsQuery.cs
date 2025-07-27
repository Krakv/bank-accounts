using bank_accounts.Features.Accounts.Dto;
using MediatR;

namespace bank_accounts.Features.Accounts.GetAccounts;

public class GetAccountsQuery(AccountFilterDto accountFilterDto) : IRequest<AccountsDto>
{
    public AccountFilterDto AccountFilterDto { get; set; } = accountFilterDto;
}