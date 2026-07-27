using Microsoft.AspNetCore.Components.Web;
using CybsClient.Services.ApiLogging;

namespace CybsClient.Components.Layout
{
    /// <summary>
    /// Extends ErrorBoundary to publish unhandled component/circuit exceptions to
    /// ApiLogHub, making them visible in the ApiLogSidebar alongside HTTP entries.
    /// Captures "errors not handled by the errorhandler component" — i.e. thrown
    /// exceptions that fall outside the CallMinAPIs keyword-heuristic detection.
    /// </summary>
    public class LoggingErrorBoundary : ErrorBoundary
    {
        protected override Task OnErrorAsync(Exception exception)
        {
            ApiLogHub.Publish(new ApiLogEntry
            {
                Method = "EXCEPTION",
                Url = string.Empty,
                Kind = ApiLogKind.Error,
                IsError = true,
                ErrorMessage = $"{exception.GetType().Name}: {exception.Message}"
            });
            return base.OnErrorAsync(exception);
        }
    }
}
