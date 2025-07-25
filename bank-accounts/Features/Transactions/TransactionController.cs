using bank_accounts.Features.Transactions.Dtos;
using Microsoft.AspNetCore.Mvc;

namespace bank_accounts.Features.Transactions
{
    [ApiController]
    [Route("transactions")]
    public class TransactionsController : ControllerBase
    {
        private readonly ITransactionService _service;
        private readonly ILogger<TransactionsController> _logger;

        public TransactionsController(ITransactionService service, ILogger<TransactionsController> logger)
        {
            _service = service;
            _logger = logger;
        }

        [HttpPost]
        public async Task<IActionResult> CreateTransaction([FromBody] CreateTransactionDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var transaction = await _service.CreateTransactionAsync(dto);
                return CreatedAtAction(nameof(GetTransaction), new { id = transaction.TransactionId }, transaction);
            }
            catch (AccountNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (InsufficientFundsException ex)
            {
                return UnprocessableEntity(ex.Message);
            }
            catch (CurrencyMismatchException ex)
            {
                return BadRequest(ex.Message);
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
            var transaction = await _service.GetTransactionAsync(id);
            return transaction != null ? Ok(transaction) : NotFound();
        }
    }
}
