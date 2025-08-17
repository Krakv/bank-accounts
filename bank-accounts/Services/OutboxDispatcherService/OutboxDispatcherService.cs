using bank_accounts.Features.Outbox.Dto;
using bank_accounts.Features.Outbox.Entities;
using bank_accounts.Infrastructure.Repository;
using RabbitMQ.Client;
using System.Diagnostics;
using System.Text;
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

        var (messages, totalCount) = await outboxRepository.GetFilteredAsync(filter);

        if (totalCount > 100) logger.LogWarning("There are {EntriesCount} entries in the outgoing messages table", totalCount);

        foreach (var message in messages)
        {
            var stopwatch = Stopwatch.StartNew();
            var retryCount = 0;
            var success = false;

            do
            {
                try
                {
                    retryCount++;
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
                    success = true;

                    logger.LogInformation("Event published | EventId:{EventId} | Type:{Type} | Latency:{Latency}ms | Retry:{Retry}", message.Id, message.Type, stopwatch.ElapsedMilliseconds, retryCount - 1);
                }
                catch (Exception ex) when (retryCount < 3)
                {
                    logger.LogWarning(ex, "Publish attempt failed | EventId:{EventId} | Retry:{Retry} | Error:{Error}", message.Id, retryCount, ex.Message);
                    await Task.Delay(500 * retryCount);
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Publish failed after {MaxRetries} attempts | EventId:{EventId} | Error:{Error}", 3, message.Id, ex.ToString());
                    break;
                }
            } while (!success && retryCount < 3);
        }
    }
}