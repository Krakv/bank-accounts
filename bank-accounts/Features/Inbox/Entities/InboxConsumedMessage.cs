using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using bank_accounts.Features.Common;

namespace bank_accounts.Features.Inbox.Entities;

[Table("inbox_consumed")]
public class InboxConsumedMessage : IEntity
{
    [Key]
    public required Guid Id { get; init; } = Guid.NewGuid();
    public required DateTime ProcessedAt { get; set; } = DateTime.UtcNow;
    [StringLength(255)]
    public required string Handler { get; set; }
}