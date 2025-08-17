using System.Text;
using System.Text.Json;
using bank_accounts.Exceptions;
using bank_accounts.Features.Accounts.FrozeClientAccounts;
using bank_accounts.Features.Accounts.UnfrozeClientAccounts;
using bank_accounts.Features.Inbox.Entities;
using bank_accounts.Features.Inbox.Payloads;
using bank_accounts.Infrastructure.Repository;
using MediatR;
using Microsoft.EntityFrameworkCore;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace bank_accounts.Services.InboxDispatcherService;

public class InboxDispatcherService(ILogger<InboxDispatcherService> logger, IConnection rabbitMqConnection, IServiceScopeFactory scopeFactory) : IInboxDispatcherService
{
    public async Task ConsumeMessages()
    {
        var channel = await rabbitMqConnection.CreateChannelAsync();
        await channel.QueueDeclareAsync("account.antifraud", exclusive:false, durable:true, autoDelete:false, arguments: null);

        await channel.BasicQosAsync(prefetchSize: 0, prefetchCount: 1, global: false);

        var consumer = new AsyncEventingBasicConsumer(channel);

        consumer.ReceivedAsync += async (_, ea) =>
        {
            var body = ea.Body.ToArray();
            var message = Encoding.UTF8.GetString(body);
            var routingKey = ea.RoutingKey;
            await HandleReceivedMessage(message, routingKey);
            await channel.BasicAckAsync(ea.DeliveryTag, false);
        };

        await channel.BasicConsumeAsync(queue: "account.antifraud", autoAck:false, consumer: consumer);
    }

    private async Task HandleReceivedMessage(string? message, string routingKey)
    {
        using var scope = scopeFactory.CreateScope();
        var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();

        if (string.IsNullOrEmpty(message))
        {
            logger.LogWarning("Received message is empty.");
            return;
        }

        try
        {
            switch (routingKey)
            {
                case "client.blocked":
                {
                    var blockingPayload = JsonSerializer.Deserialize<ClientBlockingPayload>(message);
                    if (blockingPayload == null)
                    {
                        logger.LogWarning("Unable to read payload. Message skipped.");
                    }
                    else
                    {
                        await mediator.Send(new FrozeClientAccountsCommand(blockingPayload.ClientId, blockingPayload));
                        logger.LogInformation("client.blocked event published.");
                    }

                    break;
                }
                case "client.unblocked":
                {
                    var blockingPayload = JsonSerializer.Deserialize<ClientBlockingPayload>(message);
                    if (blockingPayload == null)
                    {
                        logger.LogWarning("Unable to read payload. Message skipped.");
                    }
                    else
                    {
                        await mediator.Send(new UnfrozeClientAccountsCommand(blockingPayload.ClientId, blockingPayload));
                        logger.LogInformation("client.unblocked event published.");
                    }

                    break;
                }
            }
        }
        catch (JsonException ex)
        {
            logger.LogError(ex, "JSON deserialization error.");
            await QuarantineMessageAsync(message, ex.Message);
        }
        catch (ValidationAppException ex)
        {
            logger.LogError(ex, "Validation error occurred.");
            await QuarantineMessageAsync(message, ex.Message);
        }
        catch(DbUpdateException ex)
        {
            logger.LogError(ex, "The entry in the database already exists.");
            await QuarantineMessageAsync(message, ex.Message);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unexpected error.");
        }
    }

    private async Task QuarantineMessageAsync(string? payload, string errorMessage)
    {
        using var scope = scopeFactory.CreateScope();
        var inboxDeadRepository = scope.ServiceProvider.GetRequiredService<IRepository<InboxDeadMessage>>();

        var deadMessage = new InboxDeadMessage
        {
            Id = Guid.NewGuid(),
            ReceivedAt = DateTime.UtcNow,
            Handler = nameof(QuarantineMessageAsync),
            Payload = payload ?? "",
            Error = errorMessage
        };

        await inboxDeadRepository.CreateAsync(deadMessage);

        logger.LogInformation("Message went to quarantine.");
    }

}