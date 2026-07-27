
namespace CybsClient.Services.DIServices
{
    public interface ITransientToken
    {
        Guid Id { get; }
        string? Tt { get; }

        event Action? OnChange;

        void DeleteTt();
        void SetTt(string? value);
    }
}