namespace bank_accounts.Services.OutboxDispatcherService;

public interface IOutboxDispatcherService
{
    Task PublishPendingMessages();
}