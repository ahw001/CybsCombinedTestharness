using CybsClient.Services.ApiLogging;

namespace CybsClient.Services.DIServices
{
    /// <summary>
    /// Scoped DI service that collects API log entries for the current Blazor Server circuit.
    /// Subscribes to ApiLogHub.OnEntry in the constructor and unsubscribes on Dispose.
    /// Mirrors the SessionTransactions pattern used throughout the codebase.
    /// </summary>
    public class ApiLog : IApiLog, IDisposable
    {
        private const int MaxEntries = 200;
        private readonly List<ApiLogEntry> _entries = new();

        public IReadOnlyList<ApiLogEntry> Entries => _entries;

        public event Action? OnChange;

        public ApiLog()
        {
            ApiLogHub.OnEntry += HandleEntry;
        }

        private void HandleEntry(ApiLogEntry entry)
        {
            if (_entries.Count >= MaxEntries)
                _entries.RemoveAt(0);
            _entries.Add(entry);
            NotifyStateChanged();
        }

        public void Clear()
        {
            _entries.Clear();
            NotifyStateChanged();
        }

        private void NotifyStateChanged() => OnChange?.Invoke();

        public void Dispose()
        {
            ApiLogHub.OnEntry -= HandleEntry;
        }
    }
}
