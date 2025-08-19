using bank_accounts.Services.InboxDispatcherService;

namespace bank_accounts.Services.RabbitMqBackgroundService;

public class RabbitMqBackgroundService(IInboxDispatcherService inboxDispatcher) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await inboxDispatcher.ConsumeMessages();
    }
}