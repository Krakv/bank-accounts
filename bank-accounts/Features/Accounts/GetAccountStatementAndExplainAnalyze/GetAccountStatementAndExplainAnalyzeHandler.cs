using bank_accounts.Infrastructure.Repository;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace bank_accounts.Features.Accounts.GetAccountStatementAndExplainAnalyze;

public class GetAccountStatementAndExplainAnalyzeHandler(AppDbContext context)
    : IRequestHandler<GetAccountStatementAndExplainAnalyzeQuery, string?>
{
    public async Task<string?> Handle(GetAccountStatementAndExplainAnalyzeQuery request, CancellationToken cancellationToken)
    {
        var accountId = request.AccountId;
        var startDate = request.AccountStatementRequestDto.StartDate.ToUniversalTime();
        var endDate = request.AccountStatementRequestDto.EndDate.ToUniversalTime();

        var account = await context.Accounts.FindAsync([accountId], cancellationToken);
        if (account == null)
            return null;

        var explainSql = $"""
                              EXPLAIN ANALYZE
                              SELECT *
                              FROM "Transactions"
                              WHERE "AccountId" = '{accountId}'
                                AND "Date" BETWEEN '{startDate}' AND '{endDate}';
                          """;

        var planLines = await context.Database
            .SqlQueryRaw<string>(explainSql)
            .ToListAsync(cancellationToken: cancellationToken);

        return string.Join(Environment.NewLine, planLines);
    }
}