using bank_accounts.Exceptions;
using bank_accounts.Features.Accounts.CreateAccount;
using bank_accounts.Features.Accounts.DeleteAccount;
using bank_accounts.Features.Accounts.Dtos;
using bank_accounts.Features.Accounts.GetAccount;
using bank_accounts.Features.Accounts.GetAccounts;
using bank_accounts.Features.Accounts.GetAccountStatement;
using bank_accounts.Features.Accounts.UpdateAccount;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace bank_accounts.Features.Accounts
{
    [Route("[controller]")]
    [ApiController]
    public class AccountsController(ILogger<AccountsController> logger, IMediator mediator) : ControllerBase
    {
        private readonly ILogger<AccountsController> _logger = logger;
        private readonly IMediator _mediator = mediator;

        [HttpPost]
        public async Task<IActionResult> CreateAccount([FromBody] CreateAccountDto createAccountDto)
        {
            try
            {
                var id = await _mediator.Send(new CreateAccountCommand(createAccountDto), CancellationToken.None);

                return CreatedAtAction(
                    nameof(GetAccount),
                    new { id = id },
                    null
                );
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
                _logger.LogError(ex, "Failed to Post new Account.");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetAccount(Guid id)
        {
            try
            {
                var accountDto = await _mediator.Send(new GetAccountQuery(id), CancellationToken.None);
                return accountDto == null ? NotFound() : Ok(accountDto);
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
                _logger.LogError(ex, "Error getting account {Id}", id);
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetAccounts([FromQuery] AccountFilterDto filter)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var result = await _mediator.Send(new GetAccountsQuery(filter), CancellationToken.None);

                return Ok(result);
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
                _logger.LogError(ex, "Failed to get filtered accounts");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpPatch("{id}")]
        public async Task<IActionResult> UpdateAccountInterestRate(Guid id, [FromBody] UpdateAccountDto updateDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var account = await _mediator.Send(new GetAccountQuery(id), CancellationToken.None);

                if (account == null)
                {
                    return NotFound("Account was not found");
                }

                await _mediator.Send(new UpdateAccountCommand(id, updateDto, account.Type), CancellationToken.None);

                return Ok();
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
                _logger.LogError(ex, "Error updating interest rate for account {Id}", id);
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> CloseAccount(Guid id)
        {
            try
            {
                await _mediator.Send(new DeleteAccountCommand(id), CancellationToken.None);
                return Ok();
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
                _logger.LogError(ex, "Error closing account {Id}", id);
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpGet("{accountId}/statement")]
        public async Task<IActionResult> GetAccountStatement(Guid accountId, [FromQuery] AccountStatementRequestDto request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var statement = await _mediator.Send(new GetAccountStatementQuery(accountId, request),
                    CancellationToken.None);

                return Ok(statement);
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
                _logger.LogError(ex, "Error getting account statement");
                return StatusCode(500, "Internal server error");
            }
        }
    }
}