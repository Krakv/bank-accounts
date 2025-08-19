using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using bank_accounts.Features.Common;

namespace bank_accounts.Features.Inbox.Entities;

[Table("inbox_dead_letters")]
public class InboxDeadMessage : IEntity
{
    [Key]
    public required Guid Id { get; init; } = Guid.NewGuid();
    public required DateTime ReceivedAt { get; set; } = DateTime.UtcNow;
    [StringLength(255)]
    public required string Handler { get; set; }
    [StringLength(4000)]
    public required string Payload { get; set; }
    [StringLength(1000)]
    public required string Error { get; set; }
}