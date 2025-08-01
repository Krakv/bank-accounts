using bank_accounts.Exceptions;
using bank_accounts.Features.Common;
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
	/// <response code="500">Внутренняя ошибка сервера</response>
	[HttpPost]
	[ProducesResponseType(typeof(MbResult<List<Guid>>), StatusCodes.Status201Created)]
	[ProducesResponseType(typeof(MbResult<object>), StatusCodes.Status400BadRequest)]
	[ProducesResponseType(typeof(MbResult<object>), StatusCodes.Status404NotFound)]
	[ProducesResponseType(typeof(MbResult<object>), StatusCodes.Status500InternalServerError)]
	public async Task<IActionResult> CreateTransaction([FromBody] CreateTransactionDto dto)
	{
		try
		{
			var accountDto = await mediator.Send(new GetAccountQuery(dto.AccountId), CancellationToken.None);

			if (accountDto == null)
			{
				var notFoundResult = new MbResult<object>(
					"Account not found",
					StatusCodes.Status404NotFound,
					new Dictionary<string, string> { { "Account", $"Account with id {dto.AccountId} was not found" } }
				);
				return NotFound(notFoundResult);
			}

			var counterpartyDto = !dto.CounterpartyAccountId.HasValue
				? null
				: await mediator.Send(new GetAccountQuery(dto.CounterpartyAccountId.Value), CancellationToken.None);

			var transactionIds = await mediator.Send(new CreateTransactionCommand(dto, accountDto, counterpartyDto));

			if (transactionIds == null)
			{
				var errorResult = new MbResult<object>(
					"Transaction creation failed",
					StatusCodes.Status400BadRequest,
					new Dictionary<string, string> { { "Transaction", "Failed to create transaction" } }
				);
				return BadRequest(errorResult);
			}

			var successResult = new MbResult<Guid[]>(
				"Transaction created successfully",
				StatusCodes.Status201Created,
				transactionIds
			);

			return CreatedAtAction(
				nameof(GetTransaction),
				new { id = transactionIds[0] },
				successResult
			);
		}
		catch (ValidationAppException ex)
		{
			var result = new MbResult<object>(
				"Validation errors occurred",
				StatusCodes.Status400BadRequest,
				ex.Errors
			);
			return BadRequest(result);
		}
		catch (Exception ex)
		{
			logger.LogError(ex, "Error creating transaction");
			var result = new MbResult<object>(
				"Internal server error",
				StatusCodes.Status500InternalServerError,
				new Dictionary<string, string> { { "Error", "An unexpected error occurred while creating transaction" } }
			);
			return StatusCode(500, result);
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
	[ProducesResponseType(typeof(MbResult<object>), StatusCodes.Status400BadRequest)]
	[ProducesResponseType(typeof(MbResult<object>), StatusCodes.Status404NotFound)]
	[ProducesResponseType(typeof(MbResult<object>), StatusCodes.Status500InternalServerError)]
	public async Task<IActionResult> GetTransaction(Guid id)
	{
		try
		{
			var transaction = await mediator.Send(new GetTransactionQuery(id), CancellationToken.None);

			if (transaction == null)
			{
				var notFoundResult = new MbResult<object>(
					"Transaction not found",
					StatusCodes.Status404NotFound,
					new Dictionary<string, string> { { "Transaction", $"Transaction with id {id} was not found" } }
				);
				return NotFound(notFoundResult);
			}

			var successResult = new MbResult<TransactionDto>(
				"Transaction retrieved successfully",
				StatusCodes.Status200OK,
				transaction
			);
			return Ok(successResult);
		}
		catch (ValidationAppException ex)
		{
			var result = new MbResult<object>(
				"Validation errors occurred",
				StatusCodes.Status400BadRequest,
				ex.Errors
			);
			return BadRequest(result);
		}
		catch (Exception ex)
		{
			logger.LogError(ex, "Error getting transaction");
			var result = new MbResult<object>(
				"Internal server error",
				StatusCodes.Status500InternalServerError,
				new Dictionary<string, string> { { "Error", "An unexpected error occurred while retrieving transaction" } }
			);
			return StatusCode(500, result);
		}
	}
}