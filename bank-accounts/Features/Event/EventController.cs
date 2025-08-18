using bank_accounts.Exceptions;
using bank_accounts.Features.Common;
using bank_accounts.Features.Inbox.Entities;
using bank_accounts.Features.Inbox.GetInboxConsumedMessages;
using bank_accounts.Features.Inbox.GetInboxDeadMessages;
using bank_accounts.Features.Inbox.Payloads;
using bank_accounts.Features.Outbox.Entities;
using bank_accounts.Features.Outbox.GetOutboxMessages;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RabbitMQ.Client;
using System.Text;
using System.Text.Json;
using bank_accounts.Features.Inbox.Dto;
using bank_accounts.Features.Outbox.Dto;

namespace bank_accounts.Features.Event;

[Route("[controller]")]
[ApiController]
[Authorize]
public class EventController(IMediator mediator, ILogger<EventController> logger, ConnectionFactory connectionFactory) : ControllerBase
{
    /// <summary>
    /// Получить список необработанных сообщений (dead letters)
    /// </summary>
    /// <param name="filter">Параметры фильтрации</param>
    /// <response code="200">Список dead letters</response>
    /// <response code="404">Сообщения не найдены</response>
    /// <response code="500">Внутренняя ошибка сервера</response>
    [HttpGet("dead-letters")]
    [ProducesResponseType(typeof(MbResult<InboxDeadMessage[]>), 200)]
    public async Task<IActionResult> GetDeadLetters([FromQuery] InboxDeadMessagesFilter filter)
    {
        try
        {
            var messages = await mediator.Send(new GetInboxDeadMessagesQuery(filter));

            var result = new MbResult<InboxDeadMessage[]>(
                "Dead letters retrieved successfully",
                StatusCodes.Status200OK,
                messages
            );
            return Ok(result);
        }
        catch (NotFoundAppException ex)
        {
            var notFoundResult = new MbResult<object>(
                ex.Message,
                StatusCodes.Status404NotFound,
                new Dictionary<string, string> { { ex.EntityName ?? "Entity", ex.Message } }
            );
            return NotFound(notFoundResult);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error getting dead letters");
            var result = new MbResult<object>(
                "Internal server error",
                StatusCodes.Status500InternalServerError,
                new Dictionary<string, string> { { "Error", "An unexpected error occurred" } }
            );
            return StatusCode(500, result);
        }
    }

    /// <summary>
    /// Получить список обработанных сообщений
    /// </summary>
    /// <param name="filter">Параметры фильтрации</param>
    /// <response code="200">Список обработанных сообщений</response>
    /// <response code="404">Сообщения не найдены</response>
    /// <response code="500">Внутренняя ошибка сервера</response>
    [HttpGet("consumed-messages")]
    [ProducesResponseType(typeof(MbResult<InboxConsumedMessage[]>), 200)]
    public async Task<IActionResult> GetConsumedMessages([FromQuery] InboxConsumedMessagesFilter filter)
    {
        try
        {
            var messages = await mediator.Send(new GetInboxConsumedMessagesQuery(filter));

            var result = new MbResult<InboxConsumedMessage[]>(
                "Consumed messages retrieved successfully",
                StatusCodes.Status200OK,
                messages
            );
            return Ok(result);
        }
        catch (NotFoundAppException ex)
        {
            var notFoundResult = new MbResult<object>(
                ex.Message,
                StatusCodes.Status404NotFound,
                new Dictionary<string, string> { { ex.EntityName ?? "Entity", ex.Message } }
            );
            return NotFound(notFoundResult);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error getting consumed messages");
            var result = new MbResult<object>(
                "Internal server error",
                StatusCodes.Status500InternalServerError,
                new Dictionary<string, string> { { "Error", "An unexpected error occurred" } }
            );
            return StatusCode(500, result);
        }
    }

    /// <summary>
    /// Получить список исходящих сообщений
    /// </summary>
    /// <param name="filter">Параметры фильтрации</param>
    /// <response code="200">Список исходящих сообщений</response>
    /// <response code="404">Сообщения не найдены</response>
    /// <response code="500">Внутренняя ошибка сервера</response>
    [HttpGet("outbox-messages")]
    [ProducesResponseType(typeof(MbResult<OutboxMessage[]>), 200)]
    public async Task<IActionResult> GetOutboxMessages([FromQuery] GetOutboxMessagesFilter filter)
    {
        try
        {
            var messages = await mediator.Send(new GetOutboxMessagesQuery(filter));

            var result = new MbResult<OutboxMessage[]>(
                "Outbox messages retrieved successfully",
                StatusCodes.Status200OK,
                messages
            );
            return Ok(result);
        }
        catch (NotFoundAppException ex)
        {
            var notFoundResult = new MbResult<object>(
                ex.Message,
                StatusCodes.Status404NotFound,
                new Dictionary<string, string> { { ex.EntityName ?? "Entity", ex.Message } }
            );
            return NotFound(notFoundResult);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error getting outbox messages");
            var result = new MbResult<object>(
                "Internal server error",
                StatusCodes.Status500InternalServerError,
                new Dictionary<string, string> { { "Error", "An unexpected error occurred" } }
            );
            return StatusCode(500, result);
        }
    }

    /// <summary>
    /// Отправляет событие блокировки клиента в RabbitMQ
    /// </summary>
    /// <remarks>
    /// <para>Параметры запроса:</para>
    /// 
    /// <para>- ClientId - Идентификатор клиента (обязательный)</para>
    /// <para>- Meta - Метаданные события (CorrelationId, CausationId)</para>
    /// <para>- EventId - Идентификатор события</para>
    /// <para>- OccuredAt - Время события</para>
    /// </remarks>
    /// <param name="payload">Данные для блокировки клиента</param>
    /// <response code="200">Событие успешно отправлено</response>
    [HttpPost("block-client")]
    [ProducesResponseType(typeof(string), 200)]
    public async Task<IActionResult> BlockClient([FromBody] ClientBlockingPayload payload)
    {
        await PublishMessage("client.blocked", payload);
        logger.LogInformation("Block event sent for client {ClientId}", payload.ClientId);
        return Ok($"Block event sent for client {payload.ClientId}");
    }

    /// <summary>
    /// Отправляет событие разблокировки клиента в RabbitMQ
    /// </summary>
    /// <remarks>
    /// <para>Параметры запроса:</para>
    /// 
    /// <para>- ClientId - Идентификатор клиента (обязательный)</para>
    /// <para>- Meta - Метаданные события (CorrelationId, CausationId)</para>
    /// <para>- EventId - Идентификатор события</para>
    /// <para>- OccuredAt - Время события</para>
    /// </remarks>
    /// <param name="payload">Данные для разблокировки клиента</param>
    /// <response code="200">Событие успешно отправлено</response>
    [HttpPost("unblock-client")]
    [ProducesResponseType(typeof(string), 200)]
    public async Task<IActionResult> UnblockClient([FromBody] ClientBlockingPayload payload)
    {
        await PublishMessage("client.unblocked", payload);
        logger.LogInformation("Unblock event sent for client {ClientId}", payload.ClientId);
        return Ok($"Unblock event sent for client {payload.ClientId}");
    }

    private async Task PublishMessage(string routingKey, ClientBlockingPayload payload)
    {
        try
        {

            await using var rabbitMqConnection = await connectionFactory.CreateConnectionAsync();
            await using var channel = await rabbitMqConnection.CreateChannelAsync();

            await channel.ExchangeDeclareAsync(
                exchange: "account.events",
                type: ExchangeType.Topic,
                durable: true);

            var message = JsonSerializer.Serialize(payload);
            var body = Encoding.UTF8.GetBytes(message);

            var props = new BasicProperties
            {
                Headers = new Dictionary<string, object>
                {
                    ["X-Correlation-Id"] = payload.Meta.CorrelationId.ToString(),
                    ["X-Causation-Id"] = payload.Meta.CausationId.ToString()
                }!
            };

            await channel.BasicPublishAsync(
                exchange: "account.events",
                routingKey: routingKey,
                basicProperties: props,
                mandatory: true,
                body: body);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error publishing message to RabbitMQ. RoutingKey: {RoutingKey}", routingKey);
            throw;
        }
    }
}