namespace bank_accounts.Features.Inbox;

/// <summary>
/// Метаданные события
/// </summary>
public class Meta
{
    /// <summary>
    /// Версия формата сообщения
    /// </summary>
    /// <example>v1</example>
    public required string Version { get; set; }

    /// <summary>
    /// Источник события
    /// </summary>
    /// <example>account.events</example>
    public required string Source { get; set; }

    /// <summary>
    /// Идентификатор корреляции 
    /// </summary>
    /// <example>3fa85f64-5717-4562-b3fc-2c963f66afa9</example>
    public required Guid CorrelationId { get; set; }

    /// <summary>
    /// Идентификатор причины события
    /// </summary>
    /// <example>3fa85f64-5717-4562-b3fc-2c963f66afa8</example>
    public required Guid CausationId { get; set; }
}