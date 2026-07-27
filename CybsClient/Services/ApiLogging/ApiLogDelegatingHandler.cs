using System.Diagnostics;
using System.Text.Json.Nodes;

namespace CybsClient.Services.ApiLogging
{
    /// <summary>
    /// DelegatingHandler attached to the "Cybs.WebApi.Service" named HttpClient.
    /// Captures every request body, response body, status, URL, method, and elapsed time
    /// for BOTH CallMinAPIs code paths (SubmitForFollowOn and the typed ApiResult<T> helpers)
    /// without any changes to CallMinAPIs.cs.
    ///
    /// Response content is buffered (LoadIntoBufferAsync) before reading so that
    /// the subsequent ReadAsStringAsync call in CallMinAPIs still works correctly.
    /// </summary>
    public class ApiLogDelegatingHandler : DelegatingHandler
    {
        protected override async Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request, CancellationToken cancellationToken)
        {
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
                string? cybersourceJson = null;
                if (!string.IsNullOrWhiteSpace(responseJson))
                {
                    try
                    {
                        var node = JsonNode.Parse(responseJson);
                        cybersourceJson = node?["cybersourceJson"]?.GetValue<string>()
                                          ?? node?["error"]?["cybersourceJson"]?.GetValue<string>();
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

                ApiLogHub.Publish(new ApiLogEntry
                {
                    Method = method,
                    Url = url,
                    StatusCode = statusCode,
                    RequestJson = requestJson,
                    ResponseJson = responseJson,
                    CybersourceJson = cybersourceJson,
                    CybsTargetUrls = cybsTargetUrls,
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
