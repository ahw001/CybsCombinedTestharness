using System.Collections.Concurrent;
using CybsClass.Cybersource.Transactions;

namespace CybsClass.WebApi.Service.Services;

// Short-lived in-memory hand-off between the CybsCallContext middleware (which stores the
// exchanges captured during a request under a fresh Guid and emits X-Cybs-Log-Id) and
// GET /api/cybslog/{id} (which the CybsClient ApiLogDelegatingHandler calls immediately
// after receiving that response). Read-once: only the handler that made the original
// request ever sees the id, and the named client has no retry pipeline, so TryTake removes
// on read. The FIFO cap is a backstop for ids that are never fetched (curl callers,
// combined-app loopback probes) — nothing here is persisted.
public static class CybsCallLogStore
{
    private const int MaxEntries = 200;

    private static readonly ConcurrentDictionary<Guid, IReadOnlyList<CybsExchange>> _entries = new();
    private static readonly ConcurrentQueue<Guid> _evictionOrder = new();

    public static Guid Add(IReadOnlyList<CybsExchange> exchanges)
    {
        var id = Guid.NewGuid();
        _entries[id] = exchanges;
        _evictionOrder.Enqueue(id);

        while (_entries.Count > MaxEntries && _evictionOrder.TryDequeue(out var oldest))
        {
            _entries.TryRemove(oldest, out _);
        }

        return id;
    }

    public static bool TryTake(Guid id, out IReadOnlyList<CybsExchange>? exchanges)
    {
        var found = _entries.TryRemove(id, out var value);
        exchanges = value;
        return found;
    }
}
