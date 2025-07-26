using bank_accounts.Features.Abstract;
using bank_accounts.Features.Transactions.Entities;

namespace bank_accounts.Features.Accounts.Dtos
{
    public class StatementFilterDto : Filter<Transaction>
    {
        public Guid AccountId { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }

        public override IQueryable<Transaction> ApplyFilters(IQueryable<Transaction> query)
        {
            return query.Where(t =>
                t.AccountId == AccountId &&
                t.Date >= StartDate &&
                t.Date <= EndDate);
        }
    }
}
