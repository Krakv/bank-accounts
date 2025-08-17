using bank_accounts.Infrastructure.Repository;
using RabbitMQ.Client;
using System.Text;
using bank_accounts.Features.Outbox.Dto;
using bank_accounts.Features.Outbox.Entities;
using static System.Text.RegularExpressions.Regex;

namespace bank_accounts.Services.OutboxDispatcherService;

public class OutboxDispatcherService(IRepository<OutboxMessage> outboxRepository, IConnection rabbitMqConnection, ILogger<OutboxDispatcherService> logger) : IOutboxDispatcherService
{
    public async Task PublishPendingMessages()
    {
        logger.LogInformation("Outbox messages have started publishing.");

        await using var channel = await rabbitMqConnection.CreateChannelAsync();
        await channel.ExchangeDeclareAsync("account.events", ExchangeType.Topic, durable: true);

        var filter = new OutboxFilterDto { Page = 1, PageSize = 100 };

        var messages = (await outboxRepository.GetFilteredAsync(filter)).data;

        foreach (var message in messages)
        {
            try
            {
                var props = new BasicProperties
                {
                    Headers = new Dictionary<string, object>
                    {
                        ["X-Correlation-Id"] = message.CorrelationId.ToString(),
                        ["X-Causation-Id"] = message.CausationId.ToString()
                    }!
                };

                await channel.BasicPublishAsync(
                    exchange: "account.events",
                    routingKey: Replace(message.Type, "(?<=[a-z])([A-Z])", ".$1").ToLower(),
                    basicProperties: props,
                    mandatory: true,
                    body: Encoding.UTF8.GetBytes(message.Payload));

                message.ProcessedAt = DateTime.UtcNow;

                await outboxRepository.SaveChangesAsync();

                logger.LogInformation("Outbox message with id={EventId} have been published.", message.Id);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error occurred while publishing event messages.");
                throw;
            }
        }
    }
}