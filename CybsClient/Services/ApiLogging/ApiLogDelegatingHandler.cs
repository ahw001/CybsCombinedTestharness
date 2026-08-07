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

            // Payload-logging suppression (apiLogSuppression.json). The rule is resolved BEFORE
            // the send so the server can be told about it on the same request: it has no copy of
            // the config, and this header is how the single client-side file reaches it. The
            // decision to actually drop the payloads is made after the response, because a failed
            // call is always logged in full.
            var suppressionRule = ApiLogSuppression.Match(method, url);
            if (suppressionRule is not null)
            {
                request.Headers.TryAddWithoutValidation(ApiLogSuppression.HeaderName, suppressionRule.HeaderValue);
            }

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
                                           && ((rootObj.TryGetPropertyValue("error", out var errNode) && errNode is not null)
                                               // CyberSource's own shape, which the server passes straight
                                               // through on some endpoints: {"errors":[{"type":"declined",...}]}
                                               // — no root "error" property, but unmistakably a failure.
                                               || (rootObj.TryGetPropertyValue("errors", out var errsNode)
                                                   && errsNode is JsonArray errsArr && errsArr.Count > 0));
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
                //
                // This fetch runs even for a suppressed endpoint, and MUST: for a CyberSource
                // decline the client hop is HTTP 200 with a body carrying no root "error", so the
                // ONLY error signal is the exchange's own status. Skipping the fetch to save a
                // round-trip made the suppression decision blind and silently swallowed declines
                // (caught in live A/B testing). Suppression discards the exchanges afterwards
                // instead — pay the round-trip, keep the error visible.
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

                // Suppression applies ONLY to a call that actually succeeded. Any non-2xx status,
                // 2XX-with-embedded-error, or failed/non-2xx CyberSource exchange logs everything,
                // because that is precisely when the payload is worth having. This mirrors
                // ApiLogEntry.HasAnyError — the same three signals the sidebar flags red.
                bool anyExchangeError = cybsExchanges?.Any(x => x.IsError || x.FaultMessage is not null) ?? false;
                bool suppressPayloads = suppressionRule is not null
                                        && statusCode < 400
                                        && !hasEmbeddedError
                                        && !anyExchangeError;

                if (suppressPayloads)
                {
                    var scope = suppressionRule!.Scope;
                    if (scope is SuppressScope.Both or SuppressScope.Request) { requestJson = null; }
                    if (scope is SuppressScope.Both or SuppressScope.Response) { responseJson = null; cybersourceJson = null; }
                    // Exchange bodies are payloads too. The URL list (CybsTargetUrls) survives —
                    // it says WHICH CyberSource endpoint was called, which is call metadata, not
                    // a payload, and stays useful on a quietened endpoint.
                    cybsExchanges = null;
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
                    PayloadSuppressed = suppressPayloads,
                    SuppressionNote = suppressPayloads ? suppressionRule!.NoteText : null,
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
