using bank_accounts.Features.Abstract;
using bank_accounts.Features.Accounts.Models;

namespace bank_accounts.Features.Accounts.Dtos
{
    public class AccountFilterDto : Filter<Account>
    {
        public Guid? OwnerId { get; set; }
        public string? Type { get; set; }
        public string? Currency { get; set; }
        public decimal? MinBalance { get; set; }
        public decimal? MaxBalance { get; set; }
        public decimal? MinInterestRate { get; set; }
        public decimal? MaxInterestRate { get; set; }
        public DateTime? OpeningDateFrom { get; set; }
        public DateTime? OpeningDateTo { get; set; }
        public DateTime? ClosingDateFrom { get; set; }
        public DateTime? ClosingDateTo { get; set; }
        public bool? IsActive { get; set; } = true;
        public List<Guid>? AccountIds { get; set; }

        public override IQueryable<Account> ApplyFilters(IQueryable<Account> query)
        {
            if (OwnerId.HasValue)
                query = query.Where(a => a.OwnerId == OwnerId.Value);

            if (!string.IsNullOrEmpty(Type))
                query = query.Where(a => a.Type == Type);

            if (!string.IsNullOrEmpty(Currency))
                query = query.Where(a => a.Currency == Currency);

            if (MinBalance.HasValue)
                query = query.Where(a => a.Balance >= MinBalance.Value);

            if (MaxBalance.HasValue)
                query = query.Where(a => a.Balance <= MaxBalance.Value);

            if (MinInterestRate.HasValue)
                query = query.Where(a => a.InterestRate >= MinInterestRate.Value);

            if (MaxInterestRate.HasValue)
                query = query.Where(a => a.InterestRate <= MaxInterestRate.Value);

            if (OpeningDateFrom.HasValue)
                query = query.Where(a => a.OpeningDate >= OpeningDateFrom.Value);

            if (OpeningDateTo.HasValue)
                query = query.Where(a => a.OpeningDate <= OpeningDateTo.Value);

            if (ClosingDateFrom.HasValue)
                query = query.Where(a => a.ClosingDate >= ClosingDateFrom.Value);

            if (ClosingDateTo.HasValue)
                query = query.Where(a => a.ClosingDate <= ClosingDateTo.Value);

            if (IsActive.HasValue)
                query = IsActive.Value
                    ? query.Where(a => a.ClosingDate == null)
                    : query.Where(a => a.ClosingDate != null);

            if (AccountIds?.Count > 0)
                query = query.Where(a => AccountIds.Contains(a.Id));

            return query;
        }
    }
}
