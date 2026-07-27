using System.Text.Json.Nodes;
using CybsClient.Model.Cybersource.BaseData;

namespace CybsClient.Services.Utilities
{
    public class ApiResult<T>
    {
        public T? Data { get; set; }
        public ErrorObject? Error { get; set; }
        public JsonNode? ResponseJson { get; set; }
        public int StatusCode { get; set; }
        public Dictionary<string, string>? ResponseHeaders { get; set; }

        public bool IsSuccess => Error is null;

        public static ApiResult<T> Success(T? data, JsonNode? responseJson, int statusCode, Dictionary<string, string>? headers = null)
            => new()
            {
                Data = data,
                ResponseJson = responseJson,
                StatusCode = statusCode,
                ResponseHeaders = headers
            };

        public static ApiResult<T> Fail(string message, string? reason = null, string? action = null,
            JsonNode? responseJson = null, int statusCode = 0, Dictionary<string, string>? headers = null)
        {
            var errorObject = new ErrorObject
            {
                Id = "0",
                Error = "Error",
                Message = message,
                Reason = reason ?? "An error occurred during the API call.",
                Action = action ?? "Please check the input and try again.",
                CreatedAt = DateTime.UtcNow,
                // Raw CyberSource response body, if the server surfaced one (top-level, or
                // nested under an "error" object). This is what makes an "INVALID JSON"
                // failure diagnosable instead of opaque.
                CybersourceJson = responseJson?["cybersourceJson"]?.GetValue<string>()
                                  ?? (responseJson?["error"] as JsonObject)?["cybersourceJson"]?.GetValue<string>()
            };

            return new ApiResult<T>
            {
                Error = errorObject,
                ResponseJson = responseJson ?? System.Text.Json.JsonSerializer.SerializeToNode(errorObject),
                StatusCode = statusCode,
                ResponseHeaders = headers
            };
        }
    }
}
