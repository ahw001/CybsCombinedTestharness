namespace CybsClient.Services.DIServices;

/// <summary>
/// Scoped service (circuit-lifetime in Blazor Server) that holds the real
/// browser client IP address, captured once by <see cref="ClientIpCircuitHandler"/>
/// during SignalR circuit establishment.
/// </summary>
public class ClientIpService
{
    public string IpAddress { get; set; } = "unknown";
}
