using System.Text.Json;

namespace CybsClass.WebApi.Service.Services
{
    public enum PayloadLogSuppression { None, Request, Response, Both }

    /// <summary>
    /// Per-request payload-logging suppression, driven by the X-Suppress-Payload-Log header that
    /// CybsClient's ApiLogDelegatingHandler sets from apiLogSuppression.json.
    ///
    /// The client-side file is the single source of truth for both projects — the server has no
    /// copy of it deliberately, so there is one place to edit and it behaves identically whether
    /// the two run as separate processes or merged into the combined host. A caller that does not
    /// send the header (curl, a server-initiated call, another service) logs in full, as before.
    ///
    /// Reset() eagerly creates the holder and later reads mutate/read that same object, matching
    /// the rule established by CybsCallContext: never rebind the AsyncLocal deeper in the call
    /// chain, because a captured ExecutionContext snapshot would not see the reassignment.
    /// </summary>
    public static class PayloadLogContext
    {
        public const string HeaderName = "X-Suppress-Payload-Log";

        private sealed class Holder
        {
            public PayloadLogSuppression Mode { get; set; } = PayloadLogSuppression.None;
        }

        private static readonly AsyncLocal<Holder?> _current = new();

        public static void Reset(string? headerValue)
        {
            _current.Value = new Holder { Mode = Parse(headerValue) };
        }

        public static PayloadLogSuppression Mode => _current.Value?.Mode ?? PayloadLogSuppression.None;

        public static bool SuppressRequest =>
            Mode is PayloadLogSuppression.Request or PayloadLogSuppression.Both;

        public static bool SuppressResponse =>
            Mode is PayloadLogSuppression.Response or PayloadLogSuppression.Both;

        private static PayloadLogSuppression Parse(string? headerValue) =>
            headerValue?.Trim().ToLowerInvariant() switch
            {
                "both" => PayloadLogSuppression.Both,
                "request" => PayloadLogSuppression.Request,
                "response" => PayloadLogSuppression.Response,
                _ => PayloadLogSuppression.None
            };
    }

    /// <summary>
    /// Console payload logging that honours <see cref="PayloadLogContext"/>. Endpoints that log
    /// inbound/outbound payloads (Server Project Conventions) should write them through here
    /// instead of a bare Console.WriteLine — that is what makes an endpoint configurable from
    /// apiLogSuppression.json. Output stays prettified per the JSON logging constraint.
    /// </summary>
    public static class PayloadLog
    {
        private static readonly JsonSerializerOptions _options = new()
        {
            WriteIndented = true,
            DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
        };

        public static void Inbound(string label, object? payload) =>
            Write(label, payload, PayloadLogContext.SuppressRequest);

        public static void Outbound(string label, object? payload) =>
            Write(label, payload, PayloadLogContext.SuppressResponse);

        private static void Write(string label, object? payload, bool suppressed)
        {
            if (suppressed)
            {
                Console.WriteLine($"{label}: success (payload logging suppressed by apiLogSuppression.json)");
                return;
            }

            try
            {
                Console.WriteLine($"{label}:\n{JsonSerializer.Serialize(payload, _options)}");
            }
            catch (Exception ex)
            {
                // Logging must never break a request.
                Console.WriteLine($"{label}: <unserializable payload: {ex.Message}>");
            }
        }
    }
}
