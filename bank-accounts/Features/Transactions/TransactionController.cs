using bank_accounts.Exceptions;
using bank_accounts.Features.Accounts.GetAccount;
using bank_accounts.Features.Transactions.CreateTransaction;
using bank_accounts.Features.Transactions.Dto;
using bank_accounts.Features.Transactions.GetTransaction;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace bank_accounts.Features.Transactions;

/// <summary>
/// Контроллер для работы с банковскими транзакциями
/// </summary>
/// <remarks>
/// <para>Обрабатываемые операции:</para> 
/// 
/// <para>Создание транзакций (пополнения/списания) </para>
/// <para>Переводы между счетами </para>
/// <para>Получение информации о транзакциях </para>
/// </remarks>
[ApiController]
[Route("transactions")]
public class TransactionsController(IMediator mediator, ILogger<TransactionsController> logger) : ControllerBase
{
    /// <summary>
    /// Создать новую транзакцию
    /// </summary>
    /// <remarks>
    /// <para>Поддерживаемые операции:</para>
    /// 
    /// <para>1. Внутренние операции</para>
    /// <para>   Пополнения/списания средств</para>
    /// <para>   Параметр: counterpartyAccountId = null</para>
    /// 
    /// <para>2. Межсчетные переводы</para>
    /// <para>   Переводы между счетами</para>
    /// <para>   Параметр: counterpartyAccountId (указание счета-получателя)</para>
    /// <para>   Результат: создает парные транзакции (списание + зачисление)</para>
    /// </remarks>
    /// <response code="201">Транзакция успешно создана</response>
    /// <response code="400">Невалидные данные запроса</response>
    /// <response code="404">Счет не найден</response>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [HttpPost]
    public async Task<IActionResult> CreateTransaction([FromBody] CreateTransactionDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        try
        {
            var accountDto = await mediator.Send(new GetAccountQuery(dto.AccountId), CancellationToken.None);

            if (accountDto == null)
            {
                return NotFound("Account was not found");
            }

            var counterpartyDto = !dto.CounterpartyAccountId.HasValue 
                ? null 
                : await mediator.Send(new GetAccountQuery(dto.CounterpartyAccountId.Value), CancellationToken.None);

            var transactionIds =
                await mediator.Send(new CreateTransactionCommand(dto, accountDto, counterpartyDto));

            if (transactionIds != null)
                return CreatedAtAction(nameof(GetTransaction), new { id = transactionIds[0] }, transactionIds);
            return NotFound("Account was not found");
        }
        catch (ValidationAppException ex)
        {
            return BadRequest(new
            {
                title = "Validation errors occurred",
                status = StatusCodes.Status400BadRequest,
                errors = ex.Errors
            });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error creating transaction");
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Получить информацию о транзакции по ID
    /// </summary>
    /// <remarks>
    /// Возвращает полные данные о конкретной транзакции по её идентификатору.
    /// </remarks>
    /// <param name="id">Идентификатор транзакции (GUID)</param>
    /// <response code="200">Возвращает данные транзакции</response>
    /// <response code="400">Невалидный ID транзакции</response>
    /// <response code="404">Транзакция не найдена</response>
    /// <response code="500">Ошибка сервера</response>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(TransactionDto), 200)]
    public async Task<IActionResult> GetTransaction(Guid id)
    {
        try
        {
            var transaction = await mediator.Send(new GetTransactionQuery(id), CancellationToken.None);
            return transaction != null ? Ok(transaction) : NotFound("Transaction was not found");
        }
        catch (ValidationAppException ex)
        {
            return BadRequest(new
            {
                title = "Validation errors occurred",
                status = StatusCodes.Status400BadRequest,
                errors = ex.Errors
            });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error getting transaction");
            return StatusCode(500, "Internal server error");
        }
    }
}