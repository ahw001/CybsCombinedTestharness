using CybsClient.Services.Utilities;
using System;
using System.Collections.Generic;
using System.Text.Json;

namespace CybsClient.Services.ErrorHandling
{
    public class BasicErrorInfo
    {
        public string? Status { get; set; }
        public string? Reason { get; set; }
        public string? Message { get; set; }
        public string? Action { get; set; }

        public override string ToString()
        {
            return $"Status: {Status}, Reason: {Reason}, Message: {Message}, Action: {Action}";
        }
    }

    public static class JsonErrorExtractor
    {
        // Only scan string values for error keywords when the property name is a
        // well-known error-signal field; scanning every string value produces false
        // positives on legitimate data fields such as "cybersourceBoardingStatus".
        private static readonly HashSet<string> _errorPropertyNames = new(StringComparer.OrdinalIgnoreCase)
        {
            "message", "error", "reason", "action", "description",
            "error_description", "detail", "details", "title"
        };

        public static List<BasicErrorInfo> ExtractErrorObjects(string json)
        {
            using var document = JsonDocument.Parse(json);
            var results = new List<BasicErrorInfo>();

            ProcessElement(document.RootElement, results);

            return results;
        }

        // Property values named "error"/"reason"/"message"/etc. are usually strings
        // (OAuth-style {"error":"invalid_grant"}), but this app's own DTO/ErrorObject
        // convention ([JsonPropertyName("error")] public ErrorObject? Error) makes
        // "error" a nested object instead — GetString() on that throws
        // InvalidOperationException. Guard every such read.
        private static string? GetStringOrNull(JsonElement element)
            => element.ValueKind == JsonValueKind.String ? element.GetString() : null;

        private static void ProcessElement(JsonElement element, List<BasicErrorInfo> results)
        {
            if (element.ValueKind == JsonValueKind.Object)
            {
                var errorInfo = new BasicErrorInfo();

                if (element.TryGetProperty("status", out var statusProp))
                    errorInfo.Status = GetStringOrNull(statusProp);

                if (element.TryGetProperty("errorInformation", out var errorInfoProp))
                {
                    if (errorInfoProp.TryGetProperty("reason", out var reasonProp))
                        errorInfo.Reason = GetStringOrNull(reasonProp);
                    if (errorInfoProp.TryGetProperty("message", out var messageProp))
                        errorInfo.Message = GetStringOrNull(messageProp);
                }

                if (element.TryGetProperty("riskInformation", out var riskProp) &&
                    riskProp.TryGetProperty("profile", out var profileProp) &&
                    profileProp.TryGetProperty("action", out var actionProp))
                {
                    errorInfo.Action = GetStringOrNull(actionProp);
                }

                // Fallback fields
                if (element.TryGetProperty("reason", out var reasonFallback))
                    errorInfo.Reason ??= GetStringOrNull(reasonFallback);
                if (element.TryGetProperty("message", out var messageFallback))
                    errorInfo.Message ??= GetStringOrNull(messageFallback);
                if (element.TryGetProperty("action", out var actionFallback))
                    errorInfo.Action ??= GetStringOrNull(actionFallback);
                if (element.TryGetProperty("error", out var errorProp))
                    errorInfo.Reason ??= GetStringOrNull(errorProp);
                if (element.TryGetProperty("error_description", out var errorDescProp))
                    errorInfo.Message ??= GetStringOrNull(errorDescProp);

                // Handle root-level or nested 'errors' array. Items are either plain strings,
                // or objects (CyberSource TMS-family shape: {"type":"declined","message":"..."}).
                // "type" is only treated as an error signal here, inside a known errors array —
                // not as a general fallback field, since success responses (e.g. tokenized-cards)
                // legitimately use a top-level "type" for card brand ("visa", "mastercard").
                if (element.TryGetProperty("errors", out var errorsArray) && errorsArray.ValueKind == JsonValueKind.Array)
                {
                    foreach (var errItem in errorsArray.EnumerateArray())
                    {
                        if (errItem.ValueKind == JsonValueKind.String)
                        {
                            var message = errItem.GetString();
                            if (!string.IsNullOrWhiteSpace(message))
                                results.Add(new BasicErrorInfo { Message = message });
                        }
                        else if (errItem.ValueKind == JsonValueKind.Object)
                        {
                            string? type = errItem.TryGetProperty("type", out var typeProp) ? typeProp.GetString() : null;
                            string? msg = errItem.TryGetProperty("message", out var msgProp) ? msgProp.GetString() : null;
                            if (!string.IsNullOrWhiteSpace(type) || !string.IsNullOrWhiteSpace(msg))
                                results.Add(new BasicErrorInfo { Reason = type, Message = msg });
                        }
                    }
                }

                bool hasErrorContent =
                    !string.IsNullOrEmpty(errorInfo.Reason) ||
                    !string.IsNullOrEmpty(errorInfo.Message) ||
                    !string.IsNullOrEmpty(errorInfo.Action);

                bool isErrorStatus = !string.IsNullOrEmpty(errorInfo.Status) &&
                    (errorInfo.Status.Contains("ERROR", StringComparison.OrdinalIgnoreCase) ||
                     errorInfo.Status.Contains("INVALID", StringComparison.OrdinalIgnoreCase) ||
                     errorInfo.Status.Contains("DECLINED", StringComparison.OrdinalIgnoreCase));

                if (hasErrorContent || isErrorStatus)
                    results.Add(errorInfo);

                // Recurse through all properties
                foreach (var property in element.EnumerateObject())
                {
                    if (property.Value.ValueKind == JsonValueKind.String)
                    {
                        string val = property.Value.GetString() ?? "";

                        if (_errorPropertyNames.Contains(property.Name) &&
                            AppErrorConfig.ErrorKeywords.Any(k =>
                                val.Contains(k, StringComparison.OrdinalIgnoreCase)))
                        {
                            results.Add(new BasicErrorInfo { Message = val });
                        }

                        if (val.StartsWith("{") && val.EndsWith("}"))
                        {
                            try
                            {
                                using var innerDoc = JsonDocument.Parse(val);
                                ProcessElement(innerDoc.RootElement, results);
                            }
                            catch { }
                        }
                    }
                    else
                    {
                        ProcessElement(property.Value, results);
                    }
                }
            }
            else if (element.ValueKind == JsonValueKind.String)
            {
                // At this level there is no property name context, so don't keyword-scan
                // bare string values — they could be any data field.
                string val = element.GetString() ?? "";

                if (val.StartsWith("{") && val.EndsWith("}"))
                {
                    try
                    {
                        using var innerDoc = JsonDocument.Parse(val);
                        ProcessElement(innerDoc.RootElement, results);
                    }
                    catch { }
                }
            }
            else if (element.ValueKind == JsonValueKind.Array)
            {
                foreach (var item in element.EnumerateArray())
                {
                    ProcessElement(item, results);
                }
            }
        }


    }
}
