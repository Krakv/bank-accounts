using bank_accounts.Exceptions;
using bank_accounts.Features.Common;
using bank_accounts.Features.Accounts.CreateAccount;
using bank_accounts.Features.Accounts.CloseAccount;
using bank_accounts.Features.Accounts.DeleteAccount;
using bank_accounts.Features.Accounts.Dto;
using bank_accounts.Features.Accounts.GetAccount;
using bank_accounts.Features.Accounts.GetAccounts;
using bank_accounts.Features.Accounts.GetAccountStatement;
using bank_accounts.Features.Accounts.UpdateAccount;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

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
        try
        {
            var id = await mediator.Send(new CreateAccountCommand(createAccountDto), CancellationToken.None);

            var result = new MbResult<Guid>(
                "Account created successfully",
                StatusCodes.Status201Created,
                id
            );

            return CreatedAtAction(
                nameof(GetAccount),
                new { id },
                result
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
            logger.LogError(ex, "Failed to Post new Account.");
            var result = new MbResult<object>(
                "Internal server error",
                StatusCodes.Status500InternalServerError,
                new Dictionary<string, string> { { "Error", "An unexpected error occurred" } }
            );
            return StatusCode(500, result);
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
        try
        {
            var accountDto = await mediator.Send(new GetAccountQuery(id), CancellationToken.None);

            if (accountDto == null)
            {
                var notFoundResult = new MbResult<object>(
                    "Account not found",
                    StatusCodes.Status404NotFound,
                    new Dictionary<string, string> { { "Account", $"Account with id {id} was not found" } }
                );
                return NotFound(notFoundResult);
            }

            var result = new MbResult<AccountDto>(
                "Account retrieved successfully",
                StatusCodes.Status200OK,
                accountDto
            );
            return Ok(result);
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
            logger.LogError(ex, "Error getting account {Id}", id);
            var result = new MbResult<object>(
                "Internal server error",
                StatusCodes.Status500InternalServerError,
                new Dictionary<string, string> { { "Error", "An unexpected error occurred" } }
            );
            return StatusCode(500, result);
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
        try
        {
            var accountsResult = await mediator.Send(new GetAccountsQuery(filter), CancellationToken.None);

            if (accountsResult.Accounts == null || !accountsResult.Accounts.Any())
            {
                var notFoundResult = new MbResult<object>(
                    "Accounts not found",
                    StatusCodes.Status404NotFound,
                    new Dictionary<string, string> { { "Accounts", "No accounts matching the criteria were found" } }
                );
                return NotFound(notFoundResult);
            }

            var successResult = new MbResult<AccountsDto>(
                "Accounts retrieved successfully",
                StatusCodes.Status200OK,
                accountsResult
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
            logger.LogError(ex, "Failed to get filtered accounts");
            var result = new MbResult<object>(
                "Internal server error",
                StatusCodes.Status500InternalServerError,
                new Dictionary<string, string> { { "Error", "An unexpected error occurred" } }
            );
            return StatusCode(500, result);
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
        try
        {
            var account = await mediator.Send(new GetAccountQuery(id), CancellationToken.None);

            if (account == null)
            {
                var notFoundResult = new MbResult<object>(
                    "Account not found",
                    StatusCodes.Status404NotFound,
                    new Dictionary<string, string> { { "Account", $"Account with id {id} was not found" } }
                );
                return NotFound(notFoundResult);
            }

            await mediator.Send(new UpdateAccountCommand(id, updateDto, account.Type), CancellationToken.None);

            var successResult = new MbResult<Guid>(
                "Account updated successfully",
                StatusCodes.Status200OK,
                id
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
            logger.LogError(ex, "Error updating interest rate for account {Id}", id);
            var result = new MbResult<object>(
                "Internal server error",
                StatusCodes.Status500InternalServerError,
                new Dictionary<string, string> { { "Error", "An unexpected error occurred" } }
            );
            return StatusCode(500, result);
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
        try
        {
            var account = await mediator.Send(new GetAccountQuery(id), CancellationToken.None);

            if (account == null)
            {
                var notFoundResult = new MbResult<object>(
                    "Account not found",
                    StatusCodes.Status404NotFound,
                    new Dictionary<string, string> { { "Account", $"Account with id {id} was not found" } }
                );
                return NotFound(notFoundResult);
            }

            await mediator.Send(new CloseAccountCommand(id), CancellationToken.None);

            var successResult = new MbResult<Guid>(
                "Account closed successfully",
                StatusCodes.Status200OK,
                id
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
            logger.LogError(ex, "Error closing account {Id}", id);
            var result = new MbResult<object>(
                "Internal server error",
                StatusCodes.Status500InternalServerError,
                new Dictionary<string, string> { { "Error", "An unexpected error occurred" } }
            );
            return StatusCode(500, result);
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
        try
        {
            var account = await mediator.Send(new GetAccountQuery(id), CancellationToken.None);

            if (account == null)
            {
                var notFoundResult = new MbResult<object>(
                    "Account not found",
                    StatusCodes.Status404NotFound,
                    new Dictionary<string, string> { { "Account", $"Account with id {id} was not found" } }
                );
                return NotFound(notFoundResult);
            }

            await mediator.Send(new DeleteAccountCommand(id), CancellationToken.None);

            var successResult = new MbResult<Guid>(
                "Account deleted successfully",
                StatusCodes.Status200OK,
                id
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
            logger.LogError(ex, "Error closing account {Id}", id);
            var result = new MbResult<object>(
                "Internal server error",
                StatusCodes.Status500InternalServerError,
                new Dictionary<string, string> { { "Error", "An unexpected error occurred" } }
            );
            return StatusCode(500, result);
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
        try
        {
            var account = await mediator.Send(new GetAccountQuery(accountId), CancellationToken.None);

            if (account == null)
            {
                var notFoundResult = new MbResult<object>(
                    "Account not found",
                    StatusCodes.Status404NotFound,
                    new Dictionary<string, string> { { "Account", $"Account with id {accountId} was not found" } }
                );
                return NotFound(notFoundResult);
            }

            var statement = await mediator.Send(new GetAccountStatementQuery(accountId, request),
                CancellationToken.None);

            var successResult = new MbResult<AccountStatementResponseDto>(
                "Account statement retrieved successfully",
                StatusCodes.Status200OK,
                statement
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
            logger.LogError(ex, "Error getting account statement");
            var result = new MbResult<object>(
                "Internal server error",
                StatusCodes.Status500InternalServerError,
                new Dictionary<string, string> { { "Error", "An unexpected error occurred" } }
            );
            return StatusCode(500, result);
        }
    }
}