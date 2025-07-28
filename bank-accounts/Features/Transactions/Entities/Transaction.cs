using System.ComponentModel.DataAnnotations;

namespace bank_accounts.Features.Transactions.Entities;

public class Transaction : IEntity
{
    public Guid Id { get; init; }
    public Guid AccountId { get; set; }
    public Guid? CounterpartyAccountId { get; set; }
    public decimal Value { get; set; }
    [MinLength(5)]
    [MaxLength(6)]
    public string Type { get; set; } = string.Empty;
    [StringLength(3)]
    public string Currency { get; set; } = string.Empty;
    [StringLength(255)]
    public string Description { get; set; } = string.Empty;
    public DateTime Date { get; set; } = DateTime.UtcNow;
}