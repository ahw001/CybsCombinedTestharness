using Microsoft.AspNetCore.Components.Server.Circuits;

namespace CybsClient.Services.DIServices;

/// <summary>
/// Captures the real browser client IP from the SignalR WebSocket upgrade
/// request and stores it in the circuit-scoped <see cref="ClientIpService"/>.
/// IHttpContextAccessor.HttpContext IS available here because OnConnectionUpAsync
/// executes within the SignalR hub method context (the HTTP upgrade request).
/// </summary>
public class ClientIpCircuitHandler : CircuitHandler
{
    private readonly ClientIpService _clientIpService;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public ClientIpCircuitHandler(
        ClientIpService clientIpService,
        IHttpContextAccessor httpContextAccessor)
    {
        _clientIpService = clientIpService;
        _httpContextAccessor = httpContextAccessor;
    }

    public override Task OnConnectionUpAsync(Circuit circuit, CancellationToken cancellationToken)
    {
        var ctx = _httpContextAccessor.HttpContext;
        if (ctx is not null)
        {
            _clientIpService.IpAddress =
                ctx.Request.Headers.TryGetValue("X-Forwarded-For", out var fwd)
                    ? fwd.ToString().Split(',')[0].Trim()
                    : ctx.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        }

        return base.OnConnectionUpAsync(circuit, cancellationToken);
    }
}
