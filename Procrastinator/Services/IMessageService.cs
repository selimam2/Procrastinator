namespace Procrastinator.Services
{
    public interface IMessageService
    {
        Task<bool> SendMessageAsync(string recipient, string message);
    }
}
