using bank_accounts.Features.Accounts.Dtos;
using Microsoft.AspNetCore.Mvc;

namespace bank_accounts.Features.Accounts
{
    [Route("[controller]")]
    [ApiController]
    public class AccountsController(IAccountService accountService, ILogger<AccountsController> logger) : ControllerBase
    {
        private readonly IAccountService _accountService = accountService;
        private readonly ILogger<AccountsController> _logger = logger;

        [HttpPost]
        public async Task<IActionResult> PostAccount([FromBody] PostAccountDto accountDto)
        {
            try
            {
                var account = await _accountService.CreateAccountAsync(accountDto);
                return CreatedAtAction(
                    actionName: nameof(GetAccount),
                    routeValues: new { id = account.Id },
                    value: account
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to Post new Account.");
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetAccount(Guid id)
        {
            try
            {
                var account = await _accountService.GetAccountAsync(id);
                var accountDto = account == null ? null : new AccountDto
                {
                    Id = account.Id,
                    OwnerId = account.OwnerId,
                    Type = account.Type,
                    Currency = account.Currency,
                    Balance = account.Balance,
                    InterestRate = account.InterestRate,
                    OpeningDate = account.OpeningDate,
                    ClosingDate = account.ClosingDate
                };
                return accountDto == null ? NotFound() : Ok(accountDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting account {Id}", id);
                return BadRequest(ex.Message);
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

                var (accounts, totalCount) = await _accountService.GetAccountsAsync(filter);

                var accountsDto = accounts == null
                    ? null
                    : accounts.Select(account => new AccountDto
                    {
                        Id = account.Id,
                        OwnerId = account.OwnerId,
                        Type = account.Type,
                        Currency = account.Currency,
                        Balance = account.Balance,
                        InterestRate = account.InterestRate,
                        OpeningDate = account.OpeningDate,
                        ClosingDate = account.ClosingDate
                    });

                var result = new
                {
                    accountsDto,
                    pagination = new
                    {
                        filter.Page,
                        filter.PageSize,
                        totalCount,
                        totalPages = (int)Math.Ceiling((double)totalCount / filter.PageSize)
                    }
                };

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get filtered accounts");
                return StatusCode(500, ex.Message);
            }
        }

        [HttpPatch("{id}")]
        public async Task<IActionResult> UpdateAccountInterestRate(Guid id, [FromBody] UpdateInterestRateDto updateDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var account = await _accountService.UpdateInterestRateAsync(id, updateDto);
                return Ok(account);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
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
                var account = await _accountService.CloseAccountAsync(id);
                return Ok(account);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
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

                if (request.StartDate > request.EndDate)
                {
                    return UnprocessableEntity("Start date cannot be after end date");
                }

                var statement = await _accountService.GetAccountStatementAsync(accountId, request);

                if (statement == null)
                {
                    return NotFound("Account not found");
                }

                return Ok(statement);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting account statement");
                return StatusCode(500, "Internal server error");
            }
        }
    }
}