using bank_accounts.Exceptions;
using bank_accounts.Features.Accounts.CloseAccount;
using bank_accounts.Features.Accounts.CreateAccount;
using bank_accounts.Features.Accounts.DeleteAccount;
using bank_accounts.Features.Accounts.Dto;
using bank_accounts.Features.Accounts.GetAccount;
using bank_accounts.Features.Accounts.GetAccounts;
using bank_accounts.Features.Accounts.GetAccountStatement;
using bank_accounts.Features.Accounts.GetAccountStatementAndExplainAnalyze;
using bank_accounts.Features.Accounts.UpdateAccount;
using bank_accounts.Features.Common;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using System.Text.Json;

namespace bank_accounts.Features.Accounts;

[Route("[controller]")]
[ApiController]
[Authorize]
public class AccountsController(ILogger<AccountsController> logger, IMediator mediator) : ControllerBase
{
    /// <summary>
    /// Создает новый банковский счет
    /// </summary>
    /// <remarks>
    /// <para>Параметры:</para>
    ///
    /// <para>- ownerId - GUID владельца счета (обязательный)</para>
    /// <para>- type - Тип счета: Deposit, Checking или Credit (обязательный)</para>
    /// <para>- currency - Валюта в формате ISO 4217 (USD, EUR, RUB) (обязательный)</para>
    /// <para>- interestRate - Процентная ставка (только для Deposit/Credit, decimal >= 0, 100 >= decimal)</para>
    /// </remarks>
    /// <param name="createAccountDto">Данные для создания счета</param>
    /// <response code="201">Возвращает ID созданного счета</response>
    /// <response code="400">Ошибки валидации</response>
    /// <response code="500">Внутренняя ошибка сервера</response>
    [HttpPost]
    [ProducesResponseType(typeof(MbResult<Guid>), 201)]
    public async Task<IActionResult> CreateAccount([FromBody] CreateAccountDto createAccountDto)
    {
        const string endpoint = nameof(CreateAccount);
        var stopwatch = Stopwatch.StartNew();

        try
        {
            logger.LogInformation("Request started | Endpoint: {Endpoint} | OwnerId: {OwnerId} | Currency: {Currency}", endpoint, createAccountDto.OwnerId, createAccountDto.Currency);

            var id = await mediator.Send(new CreateAccountCommand(createAccountDto));

            logger.LogInformation("Request succeeded | Endpoint: {Endpoint} | Duration: {Duration}ms | AccountId: {AccountId}", endpoint, stopwatch.ElapsedMilliseconds, id);

            var result = new MbResult<Guid>(
                "Account created successfully",
                StatusCodes.Status201Created,
                id
            );

            return CreatedAtAction(nameof(GetAccount), new { id }, result);
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
    /// Получает информацию о банковском счете по его идентификатору
    /// </summary>
    /// <remarks>
    /// <para>Формат ответа:</para>
    /// 
    /// <para>- id - Уникальный идентификатор счёта (GUID)</para>
    /// <para>- ownerId - ID владельца счёта (GUID)</para>
    /// <para>- type - Тип счета: Deposit, Checking или Credit</para>
    /// <para>- currency - Валюта: RUB, USD, EUR (ISO 4217)</para>
    /// <para>- balance - Текущий баланс (decimal)</para>
    /// <para>- interestRate - Процентная ставка (только для Deposit/Credit, decimal)</para>
    /// <para>- openingDate - Дата открытия (формат YYYY-MM-DD)</para>
    /// <para>- closingDate - Дата закрытия (null для активных счетов)</para>
    /// </remarks>
    /// <param name="id">GUID счёта</param>
    /// <response code="200">Возвращает данные счёта в указанном формате</response>
    /// <response code="400">Невалидный ID счёта</response>
    /// <response code="404">Счёт не найден</response>
    /// <response code="500">Ошибка сервера</response>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(MbResult<AccountDto>), 200)]
    public async Task<IActionResult> GetAccount(Guid id)
    {
        const string endpoint = nameof(GetAccount);
        var stopwatch = Stopwatch.StartNew();

        try
        {
            logger.LogDebug("Request started | Endpoint: {Endpoint} | AccountId: {AccountId}", endpoint, id);

            var accountDto = await mediator.Send(new GetAccountQuery(id));

            logger.LogInformation("Request succeeded | Endpoint: {Endpoint} | Duration: {Duration}ms", endpoint, stopwatch.ElapsedMilliseconds);

            return Ok(new MbResult<AccountDto>(
                "Account retrieved successfully",
                StatusCodes.Status200OK,
                accountDto
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

    /// <summary>
    /// Получить список счетов с фильтрацией
    /// </summary>
    /// <remarks>
    /// <para>Параметры запроса:</para>
    ///
    /// <para>- ownerId - Фильтр по ID владельца (GUID, опционально)</para>
    /// <para>- accountIds - Список ID счетов через запятую (GUID, опционально)</para>
    /// <para>- type - Тип счета: Deposit, Checking или Credit (опционально)</para>
    /// <para>- currency - Валюта: RUB, USD, EUR (ISO 4217, опционально)</para>
    /// <para>- page - Номер страницы (по умолчанию 1)</para>
    /// <para>- pageSize - Размер страницы (по умолчанию 20)</para>
    ///
    /// <para>Формат ответа для счета:</para>
    ///
    /// <para>- id - Уникальный идентификатор счёта (GUID)</para>
    /// <para>- ownerId - ID владельца счёта (GUID)</para>
    /// <para>- type - Тип счета: Deposit, Checking или Credit</para>
    /// <para>- currency - Валюта: RUB, USD, EUR (ISO 4217)</para>
    /// <para>- balance - Текущий баланс (decimal)</para>
    /// <para>- interestRate - Процентная ставка (только для Deposit/Credit, decimal)</para>
    /// <para>- openingDate - Дата открытия (формат YYYY-MM-DD)</para>
    /// <para>- closingDate - Дата закрытия (null для активных счетов)</para>
    /// </remarks>
    /// <param name="filter">Параметры фильтрации</param>
    /// <response code="200">Успешный запрос, возвращает список счетов</response>
    /// <response code="400">Ошибки валидации</response>
    /// <response code="404">Счета не найдены</response>
    /// <response code="500">Внутренняя ошибка сервера</response>
    [HttpGet]
    [ProducesResponseType(typeof(MbResult<AccountsDto>), 200)]
    public async Task<IActionResult> GetAccounts([FromQuery] AccountFilterDto filter)
    {
        const string endpoint = nameof(GetAccounts);
        var stopwatch = Stopwatch.StartNew();

        try
        {
            logger.LogDebug("Request started | Endpoint: {Endpoint} | Filter: {Filter}", endpoint, JsonSerializer.Serialize(filter));

            var accountsResult = await mediator.Send(new GetAccountsQuery(filter));

            logger.LogInformation("Request succeeded | Endpoint: {Endpoint} | Duration: {Duration}ms | Results: {Count}", endpoint, stopwatch.ElapsedMilliseconds, accountsResult.Accounts?.Count());

            return Ok(new MbResult<AccountsDto>(
                "Accounts retrieved successfully",
                StatusCodes.Status200OK,
                accountsResult
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
            logger.LogWarning("Not found | Endpoint: {Endpoint} | Entity: {Entity}", endpoint, ex.EntityName);

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

    /// <summary>
    /// Обновить данные счета
    /// </summary>
    /// <param name="id">Идентификатор счета (GUID)</param>
    /// <param name="updateDto">Поля для обновления</param>
    /// <response code="200">Успешный запрос, обновляет счет</response>
    /// <response code="400">Ошибки валидации</response>
    /// <response code="404">Счет не найден</response>
    /// <response code="500">Внутренняя ошибка сервера</response>
    [HttpPatch("{id:guid}")]
    [ProducesResponseType(typeof(MbResult<object>), 200)]
    public async Task<IActionResult> UpdateAccountInterestRate(Guid id, [FromBody] UpdateAccountDto updateDto)
    {
        const string endpoint = nameof(UpdateAccountInterestRate);
        var stopwatch = Stopwatch.StartNew();
        var logContext = new { Endpoint = endpoint, AccountId = id, UpdateDto = updateDto };

        try
        {
            logger.LogDebug("Update interest rate started | {Context}", logContext);

            var account = await mediator.Send(new GetAccountQuery(id), CancellationToken.None);
            await mediator.Send(new UpdateAccountCommand(id, updateDto, account.Type), CancellationToken.None);

            logger.LogInformation("Update interest rate succeeded | {Context} | Duration: {Duration}ms", logContext, stopwatch.ElapsedMilliseconds);

            return Ok(new MbResult<Guid>(
                "Account updated successfully",
                StatusCodes.Status200OK,
                id
            ));
        }
        catch (ValidationAppException ex)
        {
            logger.LogWarning("Validation failed | {Context} | Errors: {Errors}", logContext, ex.Errors);

            return BadRequest(new MbResult<object>(
                "Validation errors occurred",
                StatusCodes.Status400BadRequest,
                ex.Errors
            ));
        }
        catch (NotFoundAppException ex)
        {
            logger.LogWarning("Account not found | {Context} | Entity: {Entity}", logContext, ex.EntityName);

            return NotFound(new MbResult<object>(
                ex.Message,
                StatusCodes.Status404NotFound,
                new Dictionary<string, string> { { ex.EntityName ?? "Entity", ex.Message } }
            ));
        }
        catch (DbUpdateConcurrencyException ex)
        {
            logger.LogWarning("Concurrency conflict | {Context} | Duration: {Duration}ms", logContext, stopwatch.ElapsedMilliseconds);

            return Conflict(new MbResult<object>(
                "Account was updated by another request",
                StatusCodes.Status409Conflict,
                ex.Message
            ));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Update interest rate failed | {Context} | Duration: {Duration}ms", logContext, stopwatch.ElapsedMilliseconds);

            return StatusCode(500, new MbResult<object>(
                "Internal server error",
                StatusCodes.Status500InternalServerError,
                new Dictionary<string, string> { { "Error", "An unexpected error occurred" } }
            ));
        }
    }

    /// <summary>
    /// Закрыть счет
    /// </summary>
    /// <remarks>Запись о счете не удаляется, обновляется поле closingDate</remarks>
    /// <param name="id">GUID счёта</param>
    /// <response code="200">Успешный запрос, закрывает счет</response>
    /// <response code="400">Ошибки валидации</response>
    /// <response code="404">Счет не найден</response>
    /// <response code="500">Внутренняя ошибка сервера</response>
    [HttpPatch("{id:guid}/close")]
    [ProducesResponseType(typeof(MbResult<object>), 200)]
    public async Task<IActionResult> CloseAccount(Guid id)
    {
        const string endpoint = nameof(CloseAccount);
        var stopwatch = Stopwatch.StartNew();
        var logContext = new { Endpoint = endpoint, AccountId = id };

        try
        {
            logger.LogDebug("Close account started | {Context}", logContext);

            await mediator.Send(new CloseAccountCommand(id), CancellationToken.None);

            logger.LogInformation("Close account succeeded | {Context} | Duration: {Duration}ms", logContext, stopwatch.ElapsedMilliseconds);

            return Ok(new MbResult<Guid>(
                "Account closed successfully",
                StatusCodes.Status200OK,
                id
            ));
        }
        catch (ValidationAppException ex)
        {
            logger.LogWarning("Validation failed | {Context} | Errors: {Errors}", logContext, ex.Errors);

            return BadRequest(new MbResult<object>(
                "Validation errors occurred",
                StatusCodes.Status400BadRequest,
                ex.Errors
            ));
        }
        catch (DbUpdateConcurrencyException ex)
        {
            logger.LogWarning("Concurrency conflict | {Context} | Duration: {Duration}ms", logContext, stopwatch.ElapsedMilliseconds);

            return Conflict(new MbResult<object>(
                "Account was updated by another request",
                StatusCodes.Status409Conflict,
                ex.Message
            ));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Close account failed | {Context} | Duration: {Duration}ms", logContext, stopwatch.ElapsedMilliseconds);

            return StatusCode(500, new MbResult<object>(
                "Internal server error",
                StatusCodes.Status500InternalServerError,
                new Dictionary<string, string> { { "Error", "An unexpected error occurred" } }
            ));
        }
    }

    /// <summary>
    /// Удалить счет
    /// </summary>
    /// <remarks>Полностью удаляет счет из системы</remarks>
    /// <param name="id">Идентификатор счета (GUID)</param>
    /// <response code="200">Счет успешно удален</response>
    /// <response code="400">Ошибка валидации идентификатора счета</response>
    /// <response code="404">Счет с указанным ID не найден</response>
    /// <response code="500">Внутренняя ошибка сервера</response>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(typeof(MbResult<object>), 200)]
    public async Task<IActionResult> DeleteAccount(Guid id)
    {
        const string endpoint = nameof(DeleteAccount);
        var stopwatch = Stopwatch.StartNew();
        var logContext = new { Endpoint = endpoint, AccountId = id };

        try
        {
            logger.LogDebug("Delete account started | {Context}", logContext);

            await mediator.Send(new DeleteAccountCommand(id), CancellationToken.None);

            logger.LogInformation("Delete account succeeded | {Context} | Duration: {Duration}ms", logContext, stopwatch.ElapsedMilliseconds);

            return Ok(new MbResult<Guid>(
                "Account deleted successfully",
                StatusCodes.Status200OK,
                id
            ));
        }
        catch (ValidationAppException ex)
        {
            logger.LogWarning("Validation failed | {Context} | Errors: {Errors}", logContext, ex.Errors);

            return BadRequest(new MbResult<object>(
                "Validation errors occurred",
                StatusCodes.Status400BadRequest,
                ex.Errors
            ));
        }
        catch (DbUpdateConcurrencyException ex)
        {
            logger.LogWarning("Concurrency conflict | {Context} | Duration: {Duration}ms", logContext, stopwatch.ElapsedMilliseconds);

            return Conflict(new MbResult<object>(
                "Account was updated by another request",
                StatusCodes.Status409Conflict,
                ex.Message
            ));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Delete account failed | {Context} | Duration: {Duration}ms", logContext, stopwatch.ElapsedMilliseconds);

            return StatusCode(500, new MbResult<object>(
                "Internal server error",
                StatusCodes.Status500InternalServerError,
                new Dictionary<string, string> { { "Error", "An unexpected error occurred" } }
            ));
        }
    }

    /// <summary>
    /// Получить выписку по счету за указанный период
    /// </summary>
    /// <param name="accountId">Идентификатор счета (GUID)</param>
    /// <param name="request">Параметры запроса выписки</param>
    /// <response code="200">Возвращает выписку по счету</response>
    /// <response code="400">Невалидные параметры запроса</response>
    /// <response code="404">Счет не найден</response>
    /// <response code="500">Внутренняя ошибка сервера</response>
    [HttpGet("{accountId:guid}/statement")]
    [ProducesResponseType(typeof(MbResult<AccountStatementResponseDto>), 200)]
    public async Task<IActionResult> GetAccountStatement(Guid accountId, [FromQuery] AccountStatementRequestDto request)
    {
        const string endpoint = nameof(GetAccountStatement);
        var stopwatch = Stopwatch.StartNew();
        var logContext = new { Endpoint = endpoint, AccountId = accountId, Request = request };

        try
        {
            logger.LogDebug("Get account statement started | {Context}", logContext);

            var statement = await mediator.Send(new GetAccountStatementQuery(accountId, request), CancellationToken.None);

            logger.LogInformation("Get account statement succeeded | {Context} | Duration: {Duration}ms", logContext, stopwatch.ElapsedMilliseconds);

            return Ok(new MbResult<AccountStatementResponseDto>(
                "Account statement retrieved successfully",
                StatusCodes.Status200OK,
                statement
            ));
        }
        catch (ValidationAppException ex)
        {
            logger.LogWarning("Validation failed | {Context} | Errors: {Errors}", logContext, ex.Errors);

            return BadRequest(new MbResult<object>(
                "Validation errors occurred",
                StatusCodes.Status400BadRequest,
                ex.Errors
            ));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Get account statement failed | {Context} | Duration: {Duration}ms", logContext, stopwatch.ElapsedMilliseconds);

            return StatusCode(500, new MbResult<object>(
                "Internal server error",
                StatusCodes.Status500InternalServerError,
                new Dictionary<string, string> { { "Error", "An unexpected error occurred" } }
            ));
        }
    }

    /// <summary>
    /// Получить выписку по счету за указанный период с Explain Analyze
    /// </summary>
    /// <param name="accountId">Идентификатор счета (GUID)</param>
    /// <param name="request">Параметры запроса выписки</param>
    /// <response code="200">Возвращает выписку по счету</response>
    /// <response code="400">Невалидные параметры запроса</response>
    /// <response code="404">Счет не найден</response>
    /// <response code="500">Внутренняя ошибка сервера</response>
    [HttpGet("{accountId:guid}/statement/explain_analyze")]
    [ProducesResponseType(typeof(MbResult<string>), 200)]
    public async Task<IActionResult> GetAccountStatementWithExplainAnalyze(Guid accountId, [FromQuery] AccountStatementRequestDto request)
    {
        const string endpoint = nameof(GetAccountStatementWithExplainAnalyze);
        var stopwatch = Stopwatch.StartNew();
        var logContext = new { Endpoint = endpoint, AccountId = accountId, Request = request };

        try
        {
            logger.LogDebug("Get account statement with explain analyze started | {Context}", logContext);

            var statement = await mediator.Send(new GetAccountStatementAndExplainAnalyzeQuery(accountId, request), CancellationToken.None);

            logger.LogInformation("Get account statement with explain analyze succeeded | {Context} | Duration: {Duration}ms", logContext, stopwatch.ElapsedMilliseconds);

            return Ok(new MbResult<string>(
                "Account statement retrieved successfully",
                StatusCodes.Status200OK,
                statement
            ));
        }
        catch (ValidationAppException ex)
        {
            logger.LogWarning("Validation failed | {Context} | Errors: {Errors}", logContext, ex.Errors);

            return BadRequest(new MbResult<object>(
                "Validation errors occurred",
                StatusCodes.Status400BadRequest,
                ex.Errors
            ));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Get account statement with explain analyze failed | {Context} | Duration: {Duration}ms", logContext, stopwatch.ElapsedMilliseconds);

            return StatusCode(500, new MbResult<object>(
                "Internal server error",
                StatusCodes.Status500InternalServerError,
                new Dictionary<string, string> { { "Error", "An unexpected error occurred" } }
            ));
        }
    }
}