using System.Diagnostics;
using System.Text.Json.Nodes;
using CybsClient.Services.DTOs;
using CybsClient.Services.Utilities;

namespace CybsClient.Services.ApiLogging
{
    /// <summary>
    /// DelegatingHandler attached to the "Cybs.WebApi.Service" named HttpClient.
    /// Captures every request body, response body, status, URL, method, and elapsed time
    /// for BOTH CallMinAPIs code paths (SubmitForFollowOn and the typed ApiResult<T> helpers)
    /// without any changes to CallMinAPIs.cs.
    ///
    /// When the response carries X-Cybs-Log-Id, the server made CyberSource call(s) during
    /// this request; their full request/response capture is fetched from GET /api/cybslog/{id}
    /// and attached to the entry, so the sidebar can show the server&lt;-&gt;CyberSource hop.
    ///
    /// Response content is buffered (LoadIntoBufferAsync) before reading so that
    /// the subsequent ReadAsStringAsync call in CallMinAPIs still works correctly.
    /// </summary>
    public class ApiLogDelegatingHandler : DelegatingHandler
    {
        protected override async Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request, CancellationToken cancellationToken)
        {
            // The cybslog fetch itself (made below via CallMinAPIs) rides this same named
            // client, so it re-enters this handler. Pass it through unlogged — it is sidebar
            // plumbing, not application traffic. (Recursion couldn't loop anyway: /api/cybslog
            // makes no CyberSource calls, so its response never carries X-Cybs-Log-Id.)
            if (request.RequestUri?.AbsolutePath.Contains("/api/cybslog/", StringComparison.OrdinalIgnoreCase) == true)
            {
                return await base.SendAsync(request, cancellationToken);
            }

            // Capture request body. StringContent / ByteArrayContent is re-readable; safe to call here.
            string? requestJson = null;
            if (request.Content is not null)
            {
                try { requestJson = await request.Content.ReadAsStringAsync(cancellationToken); }
                catch { /* non-fatal — log without request body */ }
            }

            string method = request.Method.Method;
            string url = request.RequestUri?.PathAndQuery ?? request.RequestUri?.ToString() ?? string.Empty;
            var sw = Stopwatch.StartNew();

            try
            {
                var response = await base.SendAsync(request, cancellationToken);
                sw.Stop();

                int statusCode = (int)response.StatusCode;
                string? responseJson = null;

                try
                {
                    // LoadIntoBufferAsync ensures the response body is in a reusable MemoryStream.
                    // This is a no-op when HttpCompletionOption.ResponseContentRead (the default) is used.
                    // After this call, ReadAsStringAsync can be called multiple times safely.
                    await response.Content.LoadIntoBufferAsync(cancellationToken);
                    responseJson = await response.Content.ReadAsStringAsync(cancellationToken);
                }
                catch { /* non-fatal — log without response body */ }

                // Best-effort extraction of the raw CyberSource JSON, if the server attached
                // one (top-level "cybersourceJson", or nested under "error"). Never throws —
                // a non-JSON responseJson (the very "INVALID JSON" case this is meant to
                // diagnose) just means no CybersourceJson is available to show.
                // Also detect the server's 2XX + ErrorObject convention: a non-null root-level
                // "error" property means an application-level failure the HTTP status hides.
                string? cybersourceJson = null;
                bool hasEmbeddedError = false;
                if (!string.IsNullOrWhiteSpace(responseJson))
                {
                    try
                    {
                        var node = JsonNode.Parse(responseJson);
                        cybersourceJson = node?["cybersourceJson"]?.GetValue<string>()
                                          ?? node?["error"]?["cybersourceJson"]?.GetValue<string>();
                        hasEmbeddedError = node is JsonObject rootObj
                                           && rootObj.TryGetPropertyValue("error", out var errNode)
                                           && errNode is not null;
                    }
                    catch { /* non-fatal — responseJson wasn't parseable JSON */ }
                }

                // Server-attached CyberSource endpoint URL(s) actually called for this request
                // (see CybsCallContext / X-Cybs-Target-Urls on the server). Absent when the
                // endpoint made no CyberSource call (pure DB reads/writes, etc.).
                List<string>? cybsTargetUrls = null;
                if (response.Headers.TryGetValues("X-Cybs-Target-Urls", out var targetUrlValues))
                {
                    cybsTargetUrls = targetUrlValues
                        .SelectMany(v => v.Split('|', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
                        .ToList();
                }

                // Full server<->CyberSource exchange capture, when the server recorded one.
                // Fetched synchronously before publishing so the entry arrives complete and
                // ordered; one extra local round-trip per CyberSource flow. Any failure here
                // (evicted id, fetch error, DTO error) silently degrades to the URL-list
                // fallback — the sidebar must never break application traffic.
                IReadOnlyList<CybsExchangeDto>? cybsExchanges = null;
                if (response.Headers.TryGetValues("X-Cybs-Log-Id", out var logIdValues)
                    && Guid.TryParse(logIdValues.FirstOrDefault(), out var logId))
                {
                    try
                    {
                        var logResult = await CallMinAPIs.GetCybsCallLogAsync(logId);
                        if (logResult.Data?.Error is null && logResult.Data?.Exchanges is { Count: > 0 } fetched)
                        {
                            cybsExchanges = fetched;
                        }
                    }
                    catch { /* non-fatal — entry falls back to CybsTargetUrls display */ }
                }

                ApiLogHub.Publish(new ApiLogEntry
                {
                    Method = method,
                    Url = url,
                    StatusCode = statusCode,
                    RequestJson = requestJson,
                    ResponseJson = responseJson,
                    CybersourceJson = cybersourceJson,
                    CybsTargetUrls = cybsTargetUrls,
                    CybsExchanges = cybsExchanges,
                    HasEmbeddedError = hasEmbeddedError,
                    DurationMs = sw.ElapsedMilliseconds,
                    Kind = ApiLogKind.Request,
                    IsError = statusCode >= 400
                });

                return response;
            }
            catch (Exception ex)
            {
                sw.Stop();
                ApiLogHub.Publish(new ApiLogEntry
                {
                    Method = method,
                    Url = url,
                    DurationMs = sw.ElapsedMilliseconds,
                    Kind = ApiLogKind.Error,
                    IsError = true,
                    ErrorMessage = $"{ex.GetType().Name}: {ex.Message}"
                });
                throw;
            }
        }
    }
}
