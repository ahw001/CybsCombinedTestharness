using CybsClient.Services.ApiLogging;

namespace CybsClient.Services.DIServices
{
    public interface IApiLog
    {
        IReadOnlyList<ApiLogEntry> Entries { get; }
        event Action? OnChange;
        void Clear();
    }
}
