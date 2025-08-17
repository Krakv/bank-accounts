namespace bank_accounts.Services.InboxDispatcherService;

public interface IInboxDispatcherService
{
    Task ConsumeMessages();
}