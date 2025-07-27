namespace bank_accounts.Features.Accounts.Dto
{
    /// <summary>
    /// DTO для отображения транзакции в выписке по счету
    /// </summary>
    public class TransactionStatementDto
    {
        /// <summary>
        /// Уникальный идентификатор транзакции
        /// </summary>
        /// <example>3fa85f64-5717-4562-b3fc-2c963f66afa6</example>
        public Guid Id { get; set; }

        /// <summary>
        /// Тип транзакции: "Credit" (зачисление) или "Debit" (списание)
        /// </summary>
        /// <example>Credit</example>
        public required string Type { get; set; }

        /// <summary>
        /// Сумма транзакции (в валюте счета)
        /// </summary>
        /// <example>150.75</example>
        public decimal Value { get; set; }

        /// <summary>
        /// Описание/назначение платежа
        /// </summary>
        /// <example>Перевод от клиента</example>
        public string Description { get; set; } = string.Empty;

        /// <summary>
        /// Дата и время выполнения транзакции
        /// </summary>
        /// <example>2025-03-15T14:30:45</example>
        public DateTime Date { get; set; }

        /// <summary>
        /// Идентификатор счета-контрагента (если есть)
        /// </summary>
        /// <example>3fa85f64-5717-4562-b3fc-2c963f66afa7</example>
        public Guid? CounterpartyAccountId { get; set; }
    }
}