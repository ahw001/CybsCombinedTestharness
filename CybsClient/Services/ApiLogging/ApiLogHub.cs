namespace CybsClient.Services.ApiLogging
{
    /// <summary>
    /// Static event bus that bridges the static/pooled HTTP layer to per-circuit scoped subscribers.
    /// ApiLog (scoped DI) subscribes in its constructor and unsubscribes on Dispose.
    /// </summary>
    public static class ApiLogHub
    {
        public static event Action<ApiLogEntry>? OnEntry;

        public static void Publish(ApiLogEntry entry) => OnEntry?.Invoke(entry);
    }
}
