using bank_accounts.Exceptions;
using bank_accounts.Features.Common;
using bank_accounts.Features.Transactions.CreateTransaction;
using bank_accounts.Features.Transactions.Dto;
using bank_accounts.Features.Transactions.GetTransaction;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using System.Text.Json;

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
[Authorize]
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
    /// <para>2. Переводы между счетами</para>
    /// <para>   Параметр: counterpartyAccountId (указание счета-получателя)</para>
    /// <para>   Результат: создает парные транзакции (списание + зачисление)</para>
    /// </remarks>
    /// <response code="201">Транзакция успешно создана</response>
    /// <response code="400">Невалидные данные запроса</response>
    /// <response code="404">Счёт не найден</response>
    /// <response code="500">Внутренняя ошибка сервера</response>
    [HttpPost]
	[ProducesResponseType(typeof(MbResult<List<Guid>>), StatusCodes.Status201Created)]
    public async Task<IActionResult> CreateTransaction([FromBody] CreateTransactionDto dto)
    {
        const string endpoint = nameof(CreateTransaction);
        var stopwatch = Stopwatch.StartNew();

        try
        {
            logger.LogInformation("Request started | Endpoint: {Endpoint} | AccountId: {AccountId} | Amount: {Amount}", endpoint, dto.AccountId, dto.Value);

            var transactionIds = await mediator.Send(new CreateTransactionCommand(dto));

            logger.LogInformation("Request succeeded | Endpoint: {Endpoint} | Duration: {Duration}ms | TransactionIds: {TransactionIds}", endpoint, stopwatch.ElapsedMilliseconds, string.Join(",", transactionIds));

            var result = new MbResult<Guid[]>(
                "Transaction created successfully",
                StatusCodes.Status201Created,
                transactionIds
            );

            return CreatedAtAction(nameof(GetTransaction), new { id = transactionIds[0] }, result);
        }
        catch (ValidationAppException ex)
        {
            logger.LogWarning("Validation failed | Endpoint: {Endpoint} | Errors: {Errors}", endpoint, JsonSerializer.Serialize(ex.Errors));

            return BadRequest(new MbResult<object>(
                "Validation errors occurred",
                StatusCodes.Status400BadRequest,
                ex.Errors
            ));
        }
        catch (DbUpdateConcurrencyException ex)
        {
            logger.LogWarning("Concurrency conflict | Endpoint: {Endpoint} | AccountId: {AccountId}", endpoint, dto.AccountId);

            return Conflict(new MbResult<object>(
                "Account was updated by another request",
                StatusCodes.Status409Conflict,
                new Dictionary<string, string> { { "AccountId", ex.Message } }
            ));
        }
        catch (FrozenAccountException ex)
        {
            logger.LogWarning("Frozen account | Endpoint: {Endpoint} | AccountId: {AccountId}", endpoint, dto.AccountId);

            return Conflict(new MbResult<object>(
                "Account was frozen",
                StatusCodes.Status409Conflict,
                new Dictionary<string, string> { { "AccountId", ex.Message } }
            ));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Request failed | Endpoint: {Endpoint} | Duration: {Duration}ms", endpoint, stopwatch.ElapsedMilliseconds);

            return StatusCode(500, new MbResult<object>(
                "Internal server error",
                StatusCodes.Status500InternalServerError,
                new Dictionary<string, string> { { "Error", ex.Message } }
            ));
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
	[ProducesResponseType(typeof(MbResult<TransactionDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetTransaction(Guid id)
    {
        const string endpoint = nameof(GetTransaction);
        var stopwatch = Stopwatch.StartNew();

        try
        {
            logger.LogDebug("Request started | Endpoint: {Endpoint} | TransactionId: {TransactionId}", endpoint, id);

            var transaction = await mediator.Send(new GetTransactionQuery(id));

            logger.LogInformation("Request succeeded | Endpoint: {Endpoint} | Duration: {Duration}ms", endpoint, stopwatch.ElapsedMilliseconds);

            return Ok(new MbResult<TransactionDto>(
                "Transaction retrieved successfully",
                StatusCodes.Status200OK,
                transaction
            ));
        }
        catch (ValidationAppException ex)
        {
            logger.LogWarning("Validation failed | Endpoint: {Endpoint} | Errors: {Errors}", endpoint, JsonSerializer.Serialize(ex.Errors));

            return BadRequest(new MbResult<object>(
                "Validation errors occurred",
                StatusCodes.Status400BadRequest,
                ex.Errors
            ));
        }
        catch (NotFoundAppException ex)
        {
            logger.LogWarning("Not found | Endpoint: {Endpoint} | Entity: {Entity} | Id: {Id}", endpoint, ex.EntityName, id);

            return NotFound(new MbResult<object>(
                ex.Message,
                StatusCodes.Status404NotFound,
                new Dictionary<string, string> { { ex.EntityName ?? "Entity", ex.Message } }
            ));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Request failed | Endpoint: {Endpoint} | Duration: {Duration}ms", endpoint, stopwatch.ElapsedMilliseconds);

            return StatusCode(500, new MbResult<object>(
                "Internal server error",
                StatusCodes.Status500InternalServerError,
                new Dictionary<string, string> { { "Error", ex.Message } }
            ));
        }
    }
}
