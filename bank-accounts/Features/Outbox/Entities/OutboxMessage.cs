using bank_accounts.Features.Common;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace bank_accounts.Features.Outbox.Entities;

[Table("Outbox")]
public class OutboxMessage : IEntity
{
    [Key]
    public Guid Id { init; get; }

    [StringLength(255)]
    public required string Type { get; set; }

    [StringLength(4000)]
    [Column(TypeName = "jsonb")]
    public required string Payload { get; set; }

    public required DateTime OccurredAt { get; set; }
    public DateTime? ProcessedAt { get; set; }

    [StringLength(100)]
    public required string Source { get; set; }
    public required Guid CorrelationId { get; set; }
    public required Guid CausationId { get; set; }
}