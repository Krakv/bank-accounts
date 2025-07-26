using bank_accounts.Exceptions;
using bank_accounts.Features.Accounts.Dtos;
using bank_accounts.Features.Accounts.GetAccount;
using bank_accounts.Features.Transactions.CreateTransaction;
using bank_accounts.Features.Transactions.Dtos;
using bank_accounts.Features.Transactions.GetTransaction;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace bank_accounts.Features.Transactions
{
    [ApiController]
    [Route("transactions")]
    public class TransactionsController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly ILogger<TransactionsController> _logger;

        public TransactionsController(IMediator mediator, ILogger<TransactionsController> logger)
        {
            _mediator = mediator;
            _logger = logger;
        }

        [HttpPost]
        public async Task<IActionResult> CreateTransaction([FromBody] CreateTransactionDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var accountDto = await _mediator.Send(new GetAccountQuery(dto.AccountId), CancellationToken.None);

                var counterpartyDto = !dto.CounterpartyAccountId.HasValue 
                    ? null 
                    : await _mediator.Send(new GetAccountQuery(dto.CounterpartyAccountId.Value), CancellationToken.None);

                var transactionId =
                    await _mediator.Send(new CreateTransactionCommand(dto, accountDto, counterpartyDto));

                return CreatedAtAction(nameof(GetTransaction), new { id = transactionId }, null);
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
                _logger.LogError(ex, "Error creating transaction");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetTransaction(Guid id)
        {
            try
            {
                var transaction = await _mediator.Send(new GetTransactionQuery(id), CancellationToken.None);
                return transaction != null ? Ok(transaction) : NotFound();
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
                _logger.LogError(ex, "Error getting transaction");
                return StatusCode(500, "Internal server error");
            }
        }
    }
}
