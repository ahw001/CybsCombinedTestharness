// ============================================================================
// CybsCombinedTestharness merged host:
//   - Server half: CyberSourceServer\CybsClass.WebApi.Service\Program.cs (Minimal API, SQLite)
//   - Client half: CybsClient\Program.cs (Blazor Server)
// One process, one port. The Blazor client calls the API over a loopback HttpClient
// whose base address is derived at startup (see ResolveSelfBaseUrl) so the existing
// CallMinAPIs/ApiResult gauntlet stays fully intact.
// ============================================================================

using System.Net.Http.Headers;
using System.Text.Json.Serialization;
using CybsClass.Cybersource.Authentication;
using CybsClass.Cybersource.Transactions;
using CybsClass.EntityModels; // To use the AddCybsDbContext method.
using CybsClass.WebApi.Service;
using CybsClass.WebApi.Service.ExceptionHandlers;
using CybsClass.WebApi.Service.Services;
using CybsClass.WebApi.Service.Services.CcTransatcionProcessing;
using CybsClass.WebApi.Service.Services.Configs;
using CybsClass.WebApi.Service.Services.TokenProcessing;
using CybsClass.WebApi.Service.Services.Utilities;
using CybsClient.Components;
using CybsClient.Services.ApiLogging;
using CybsClient.Services.DIServices;
using CybsClient.Services.Utilities;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Components.Server.Circuits;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Options;
using Serilog;
using ServerConfigValues = CybsClass.WebApi.Service.Services.ConfigValues;
using ClientConfigValues = CybsClient.Services.Utilities.ConfigValues;

const string CorsPolicy = "AppCors";

var builder = WebApplication.CreateBuilder(args);

// ------- Environment diagnostics (console only) -------
var env = builder.Environment;
Console.WriteLine($"[Env] ASPNETCORE_ENVIRONMENT = {env.EnvironmentName}");
Console.WriteLine($"[Env] ContentRootPath = {env.ContentRootPath}");
Console.WriteLine($"[Env] BaseDirectory = {AppContext.BaseDirectory}");

// ------- Decide if logging should be enabled -------
static bool CanWriteDirectory(string path, out string? error)
{
    try
    {
        Directory.CreateDirectory(path);
        var probe = Path.Combine(path, $".probe-{Guid.NewGuid():N}.tmp");
        using (var fs = new FileStream(probe, FileMode.CreateNew, FileAccess.Write, FileShare.Read))
        {
            fs.WriteByte(0x42);
        }
        File.Delete(probe);
        error = null;
        return true;
    }
    catch (Exception ex)
    {
        error = ex.Message;
        return false;
    }
}

// Windows (local dev/IIS) -> C:\inetpub; Azure App Service Linux -> /home/LogFiles;
// Replit (or any other Linux host without /home write access) -> ./logs under the app.
var logRootCandidates = OperatingSystem.IsWindows()
    ? new[] { @"C:\inetpub\logs\CybsApi", Path.Combine(AppContext.BaseDirectory, "logs") }
    : new[] { "/home/LogFiles/CybsApi", Path.Combine(AppContext.BaseDirectory, "logs") };

string? LogRoot = null;
string? accessErr = null;
foreach (var candidate in logRootCandidates)
{
    if (CanWriteDirectory(candidate, out accessErr)) { LogRoot = candidate; break; }
}
var loggingEnabled = LogRoot is not null;

Console.WriteLine(loggingEnabled
    ? $"[Log] Using log root: {LogRoot}"
    : $"[Log] Logging disabled (no writable log root; last error: {accessErr})");

// ------- Wire Serilog only if writable; else no logging at all -------
if (loggingEnabled)
{
    builder.Host.UseSerilog((ctx, _, lc) =>
    {
        Directory.CreateDirectory(LogRoot!);

        // Serilog internal errors -> file (only when writable)
        Serilog.Debugging.SelfLog.Enable(m =>
        {
            try { File.AppendAllText(Path.Combine(LogRoot!, "serilog-selflog.txt"), m); } catch { }
        });

        lc.MinimumLevel.Information()
          .Enrich.FromLogContext()
          .WriteTo.File(
              Path.Combine(LogRoot!, "app-.log"),
              rollingInterval: RollingInterval.Day,
              retainedFileCountLimit: 30,
              shared: true);
    });
}
else
{
    // Remove all default Microsoft logging providers too (no logging at all)
    builder.Logging.ClearProviders();
}

// ------- CORS -------
var defaultAllowed = new[]
{
    "https://www.testccprocessor.com",
    "https://testccprocessor.com",
    "https://cybsclient.azurewebsites.net",
    "https://cybscombined.azurewebsites.net",
    "https://www.ahw001.com",
    "https://ahw001.com",
    "https://localhost:7173",
    "https://localhost:7133",
    "http://localhost:5173",
    "https://localhost:7290", // dev WASM (http)
    "http://localhost"        // IIS local
};

var allowedOrigins = builder.Configuration
    .GetSection("Cors:AllowedOrigins")
    .Get<string[]>() ?? defaultAllowed;

// Replit (or any host) can add its public origin without touching the repo.
var extraCorsOrigin = Environment.GetEnvironmentVariable("EXTRA_CORS_ORIGIN");
if (!string.IsNullOrWhiteSpace(extraCorsOrigin))
{
    allowedOrigins = allowedOrigins.Append(extraCorsOrigin.TrimEnd('/')).ToArray();
}

// ------- Loopback base URL for the Blazor client's API HttpClient -------
// The client half calls the server half over HTTP in this same process. The base
// address is derived from how Kestrel will bind (fully known before Build()):
//   1. ASPNETCORE_URLS (set by `dotnet run` from launchSettings, or by the host/Replit UI)
//      — prefer the http:// entry to avoid dev-cert trust issues on server-side loopback calls
//   2. PORT (Replit convention)
//   3. WebServiceServerURL from config (escape hatch), else Kestrel's default port
static string ResolveSelfBaseUrl(IConfiguration cfg)
{
    var urls = Environment.GetEnvironmentVariable("ASPNETCORE_URLS");
    if (!string.IsNullOrWhiteSpace(urls))
    {
        var parts = urls.Split(';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        var pick = parts.FirstOrDefault(u => u.StartsWith("http://", StringComparison.OrdinalIgnoreCase)) ?? parts[0];
        pick = pick.Replace("0.0.0.0", "localhost").Replace("[::]", "localhost")
                   .Replace("*", "localhost").Replace("+", "localhost");
        return pick.TrimEnd('/') + "/";
    }
    var port = Environment.GetEnvironmentVariable("PORT");
    if (!string.IsNullOrWhiteSpace(port)) return $"http://localhost:{port}/";
    return cfg["WebServiceServerURL"] ?? "http://localhost:5000/";
}

builder.Configuration["WebServiceServerURL"] = ResolveSelfBaseUrl(builder.Configuration);
Console.WriteLine($"[Loopback] WebServiceServerURL = {builder.Configuration["WebServiceServerURL"]}");

// ============================================================================
// Services — server half (from CybsClass.WebApi.Service Program.cs)
// ============================================================================
builder.Services.Configure<AppSettings>(builder.Configuration);
builder.Services.AddSingleton<ServerConfigValues>();

builder.Services.AddProblemDetails();

builder.Services.AddScoped<CcAuthService>();
builder.Services.AddScoped<CallNtDecrypt>();
builder.Services.AddScoped<CybsClass.WebApi.Service.Services.ApplePay.CcApplePayService>();

// Bind strongly typed settings (also available via IOptions<AppSettings>)
var appSettings = new AppSettings();
builder.Configuration.Bind(appSettings);

// Console diagnostics (safe details only)
Console.WriteLine($"P12 file = {appSettings.AuthCredentialFile.RestP12JwtCredential}");
Console.WriteLine($"KeyPass length = {appSettings.AuthCredentialFile.KeyPass?.Length ?? 0}");
Console.WriteLine($"IsPortfolioCredential = {appSettings.AuthCredentialFile.IsPortfolioCredential}");
Console.WriteLine($"MerchantID = {appSettings.AuthCredentialFile.MerchantID}");
Console.WriteLine($"KeyId = {appSettings.AuthSecretKey.KeyId}");
Console.WriteLine($"SharedSecret length = {appSettings.AuthSecretKey.SharedSecret?.Length ?? 0}");
Console.WriteLine($"BaseUrlAddress = {appSettings.BaseUrlAddress}");
Console.WriteLine($"BasePosUrlAddress = {appSettings.BasePosUrlAddress}");
Console.WriteLine($"AllowedHosts = {appSettings.AllowedHosts}");
Console.WriteLine($"AllowedOrigins = {string.Join(";", appSettings.Cors.AllowedOrigins ?? new List<string>())}");
Console.WriteLine($"AcceptanceMerchantId = {appSettings.AcceptanceDeviceInfo.AcceptanceMerchantId}");
Console.WriteLine($"AcceptanceSecret length = {appSettings.AcceptanceDeviceInfo.AcceptanceSecret?.Length ?? 0}");
Console.WriteLine($"AcceptanceDeviceSerialNumber = {appSettings.AcceptanceDeviceInfo.AcceptanceDeviceSerialNumber}");

builder.Services.AddAuthorization();
builder.Services.AddSignalR();

builder.Services.AddCustomRateLimiting(builder.Configuration);

// Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// DbContext — no ConnectionStrings section is present, so this resolves to the SQLite
// database at <BaseDirectory>\Data\CybsSampleDb.sqlite via AddCybsDbContext's default.
builder.Services.AddCybsDbContext(builder.Configuration.GetConnectionString("Default"));

// System.Text.Json defaults (Minimal API endpoints)
builder.Services.Configure<Microsoft.AspNetCore.Http.Json.JsonOptions>(options =>
{
    options.SerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
    options.SerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
});

// Exception handlers (application errors surface as 2XX + ErrorObject through the API path;
// these handlers cover unhandled pipeline exceptions)
builder.Services.AddExceptionHandler<TimeOutExceptionHandler>();
builder.Services.AddExceptionHandler<DefaultExceptionHandler>();

// CORS policy — single policy for the combined app. The Blazor UI and the API are
// same-origin here, so CORS only governs external direct API callers.
builder.Services.AddCors(options =>
{
    options.AddPolicy(CorsPolicy, p =>
        p.WithOrigins(allowedOrigins)
         .SetIsOriginAllowedToAllowWildcardSubdomains()
         .AllowAnyHeader()
         .AllowAnyMethod()
         .AllowCredentials());
});

// ============================================================================
// Services — client half (from CybsClient Program.cs)
// ============================================================================
builder.Services.AddRazorComponents().AddInteractiveServerComponents();
builder.Services.AddScoped<ICartService, CartService>();
builder.Services.AddScoped<IElectronicProductService, ElectronicProductService>();
builder.Services.AddScoped<ITransactionJson, TransactionJson>();
builder.Services.AddScoped<ICustomersScoped, CustomersScoped>();
builder.Services.AddScoped<ISessionTransactions, SessionTransactions>();
builder.Services.AddScoped<ICtxContext, CtxContext>();
builder.Services.AddScoped<ITransientToken, TransientToken>();
builder.Services.AddScoped<ITransactionModeState, TransactionModeState>();
builder.Services.AddScoped<IUnifiedCheckoutSettingsService, UnifiedCheckoutSettingsService>();
builder.Services.AddSingleton<ICardService, CardService>();
builder.Services.AddSingleton<IPayerAuthTestCardService, PayerAuthTestCardService>();
builder.Services.AddSingleton<INetworkTokenTestCardService, NetworkTokenTestCardService>();
builder.Services.AddScoped<IApiLog, ApiLog>();
builder.Services.AddTransient<ApiLogDelegatingHandler>();
builder.Services.AddHostedService<CardServiceInitializer>();
builder.Services.AddHostedService<PayerAuthTestCardServiceInitializer>();
builder.Services.AddHostedService<NetworkTokenTestCardServiceInitializer>();
builder.Services.AddSingleton<IConfiguration>(builder.Configuration);

// Capture real browser client IP via Blazor Server circuit handler.
// This avoids making an HTTPS loopback call from the server to itself.
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ClientIpService>();
builder.Services.AddScoped<CircuitHandler, ClientIpCircuitHandler>();

var configValues = new ClientConfigValues(builder.Configuration);

builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();

// Configure application cookie
builder.Services.ConfigureApplicationCookie(options =>
{
    options.Cookie.HttpOnly = true;
    options.Cookie.SecurePolicy = builder.Environment.IsDevelopment() ? CookieSecurePolicy.None : CookieSecurePolicy.None;
    options.Cookie.SameSite = SameSiteMode.Lax;
});

builder.Services.Configure<AntiforgeryOptions>(config =>
{
    config.Cookie.SecurePolicy = CookieSecurePolicy.None;
});

builder.Services.Configure<CookieTempDataProviderOptions>(config =>
{
    config.Cookie.SecurePolicy = CookieSecurePolicy.Always;
});

builder.Services.AddHttpClient(name: "Cybs.WebApi.Service",
  configureClient: options =>
  {
      options.BaseAddress = new(configValues.GetServereUrlAddress());
      options.DefaultRequestHeaders.Accept.Add(
      new MediaTypeWithQualityHeaderValue(
          "application/json", 1.0));
  }
).AddHttpMessageHandler<ApiLogDelegatingHandler>();

var app = builder.Build();

// ============================================================================
// Startup initialization
// ============================================================================

// Forwarded headers FIRST — Replit/Azure front-end proxies terminate TLS and forward
// scheme/client-IP via X-Forwarded-*. KnownNetworks/KnownProxies are cleared because
// those proxies are not loopback. This also makes NavigationManager.BaseUri report the
// real public https origin (needed for the 3-D Secure step-up ReturnUrl).
var fwdOptions = new ForwardedHeadersOptions
{
    ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
};
fwdOptions.KnownNetworks.Clear();
fwdOptions.KnownProxies.Clear();
app.UseForwardedHeaders(fwdOptions);

// Client: static HttpClient factory stash used by CallMinAPIs & friends
var httpClientFactory = app.Services.GetRequiredService<IHttpClientFactory>();
HttpClientHelper.Initialize(httpClientFactory);

// Options snapshot
var settings = app.Services.GetRequiredService<IOptions<AppSettings>>().Value;
Console.WriteLine($"[Cfg] P12={settings.AuthCredentialFile.RestP12JwtCredential}, KeyPassLen={settings.AuthCredentialFile.KeyPass?.Length ?? 0}");

// Initialize Credentials
Credentials.Initialize(
    settings.AuthCredentialFile.RestP12JwtCredential!,
    settings.AuthCredentialFile.IsPortfolioCredential ?? "false",
    settings.AuthCredentialFile.MerchantID ?? "",
    settings.AuthCredentialFile.KeyPass!,
    settings.AuthSecretKey.KeyId!,
    settings.AuthSecretKey.SharedSecret!,
    settings.BaseUrlAddress ?? ""
);

// Initialize BoardingCredentials (JWT boarding auth)
BoardingCredentials.Initialize(
    settings.AuthCredentialFile.RestP12JwtCredential!,
    settings.AuthCredentialFile.IsPortfolioCredential ?? "false",
    settings.AuthCredentialFile.MerchantID ?? "",
    settings.AuthCredentialFile.KeyPass!,
    settings.AuthSecretKey.KeyId!,
    settings.AuthSecretKey.SharedSecret!,
    settings.BaseUrlAddress ?? ""
);

// Initialize MLE credentials (SJC encryption cert + response decryption key)
var mleCfg = new MleSettings();
app.Configuration.GetSection("MleSettings").Bind(mleCfg);
Console.WriteLine($"[MLE] ResponseMleKid = {mleCfg.ResponseMleKid}");
MleCredentials.Initialize(
    mleCfg.SjcCertificatePath ?? "",
    mleCfg.ResponseMleKeyPath ?? "",
    mleCfg.ResponseMleKid ?? "",
    mleCfg.ResponseMleKeyPass,
    mleCfg.LegacyMlePrivateKeyPath,
    mleCfg.LegacyMleKid
);

// Initialize Apple Pay credentials (Payment Processing Certificate private key — merchant decryption)
var applePayCfg = new ApplePaySettings();
app.Configuration.GetSection("ApplePaySettings").Bind(applePayCfg);
CybsClass.Cybersource.Authentication.ApplePayCredentials.Initialize(
    applePayCfg.PaymentProcessingKeyPath ?? "",
    applePayCfg.KeyPass,
    applePayCfg.MerchantIdentifier,
    applePayCfg.MerchantIdentityCertPath,
    applePayCfg.MerchantIdentityCertPass,
    applePayCfg.InitiativeContext,
    applePayCfg.AdditionalPaymentProcessingKeyPaths
);

// Initialize URL endpoint method lookup (single copy at the host content root serves
// both the server's UrlEndpointConfig and the client's SimpleStringProcessor page)
UrlEndpointConfig.Initialize(Path.Combine(app.Environment.ContentRootPath, "urlEndpoints.json"));

// ============================================================================
// Middleware pipeline
// ============================================================================
if (loggingEnabled)
{
    app.UseSerilogRequestLogging();

    app.Lifetime.ApplicationStarted.Register(() =>
        Serilog.Log.Information("App started. Logging to {Path}", LogRoot));
}

// Server's typed exception handlers win globally (TimeOut + Default). Blazor circuit/UI
// errors are still handled in-circuit by the client's ErrorBoundary/ErrorHandler.
app.UseExceptionHandler();
app.UseAuthorization();

// NOTE: no UseHttpsRedirection/UseHsts — the Replit/Azure front-end proxy terminates TLS;
// redirecting here causes loops when X-Forwarded-Proto is absent. Local https dev on
// 7133 connects directly and is unaffected.

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// IMPORTANT: Private Network Access middleware MUST come before CORS/routing
app.Use(async (context, next) =>
{
    // Handle preflight requests for Private Network Access
    if (context.Request.Method == "OPTIONS" &&
        context.Request.Headers.ContainsKey("Access-Control-Request-Private-Network"))
    {
        context.Response.Headers.Append("Access-Control-Allow-Private-Network", "true");
        context.Response.Headers.Append("Access-Control-Allow-Origin", "*");
        context.Response.Headers.Append("Access-Control-Allow-Methods", "GET, POST, OPTIONS");
        context.Response.Headers.Append("Access-Control-Allow-Headers", "*");
        context.Response.StatusCode = 204;
        return;
    }

    // Always add the header for actual requests too
    if (context.Request.Headers.ContainsKey("Access-Control-Request-Private-Network"))
    {
        context.Response.Headers.Append("Access-Control-Allow-Private-Network", "true");
    }

    await next();
});

// Optional CORS debug logging — only when logging is enabled
if (loggingEnabled)
{
    app.Use(async (ctx, next) =>
    {
        if (ctx.Request.Path.StartsWithSegments("/api"))
        {
            var log = ctx.RequestServices.GetRequiredService<ILoggerFactory>()
                      .CreateLogger("CorsDebug");
            log.LogInformation("CORS check: Origin={Origin}; Method={Method}; ACRM={ACRM}; ACRH={ACRH}",
                ctx.Request.Headers.Origin.ToString(),
                ctx.Request.Method,
                ctx.Request.Headers.AccessControlRequestMethod.ToString(),
                ctx.Request.Headers.AccessControlRequestHeaders.ToString());
        }
        await next();
    });
}

app.UseCors(CorsPolicy);

// Surfaces the CyberSource exchange(s) actually performed during this request (if any) for
// display in the ApiLogSidebar: X-Cybs-Target-Urls carries the URL list (pipe-delimited;
// kept for curl diagnostics and as a client fallback), and X-Cybs-Log-Id points at the full
// request/response capture, retrievable once via GET /api/cybslog/{id}.
app.Use(async (context, next) =>
{
    CybsCallContext.Reset();
    context.Response.OnStarting(() =>
    {
        var exchanges = CybsCallContext.GetExchanges();
        if (exchanges.Count > 0)
        {
            context.Response.Headers["X-Cybs-Target-Urls"] = string.Join(" | ", exchanges.Select(e => e.Url));
            context.Response.Headers["X-Cybs-Log-Id"] = CybsCallLogStore.Add(exchanges).ToString();
        }
        return Task.CompletedTask;
    });
    await next();
});

// Client statics (Blazor wwwroot)
app.UseStaticFiles();

// Apple's Merchant ID domain-verification file has no extension, so the default static-file
// middleware (ServeUnknownFileTypes=false) won't serve it. Scoped narrowly to /.well-known so no
// other static-file behavior on the site changes. See wwwroot/.well-known/README.md.
app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(Path.Combine(app.Environment.WebRootPath, ".well-known")),
    RequestPath = "/.well-known",
    ServeUnknownFileTypes = true,
    DefaultContentType = "text/plain",
    ContentTypeProvider = new FileExtensionContentTypeProvider()
});

// ============================================================================
// Endpoints — client minimal endpoints
// ============================================================================
app.MapPost("/stepup-callback", async (HttpContext http) =>
{
    var form = await http.Request.ReadFormAsync();
    var transactionId = form["TransactionId"].ToString();
    var guid = form["MD"].ToString();

    string html;
    if (guid == "inpage")
    {
        // Consolidated flow (FlexConsolidatedCheckout) sets MD to this sentinel: its
        // transaction state lives in the Blazor circuit, so a top-level navigation here
        // would destroy everything. Instead, notify the host page and stay put — the
        // page auto-runs its validate+authorize path on the still-live circuit.
        // window.top (not window.parent) because the ACS renders this response inside
        // Cardinal's nested iframe; the host page is the top browsing context.
        var payload = System.Text.Json.JsonSerializer.Serialize(new
        {
            source = "cybs-stepup-callback",
            transactionId
        });
        html = $@"
    <html>
        <head><title>Challenge complete</title></head>
        <body>
            <script>
                window.top.postMessage({payload}, window.location.origin);
            </script>
        </body>
    </html>";
    }
    else
    {
        // Legacy flow (PaStepUp): MD carries the persisted-session GUID; the completion
        // page rehydrates state from the database on a fresh circuit, so the top-level
        // redirect is the designed hand-off.
        var redirectUrl = $"/stepup-complete?transactionId={transactionId}&guid={guid}";
        html = $@"
    <html>
        <head><title>Redirecting...</title></head>
        <body>
            <script>
                window.top.location.href = '{redirectUrl}';
            </script>
        </body>
    </html>";
    }

    http.Response.Headers.Append("Access-Control-Allow-Private-Network", "true");
    return Results.Content(html, "text/html");

})
.DisableAntiforgery();

app.MapGet("/client-ip", (HttpContext http) =>
    Results.Text(
        http.Request.Headers.TryGetValue("X-Forwarded-For", out var fwd)
            ? fwd.ToString().Split(',')[0].Trim()
            : http.Connection.RemoteIpAddress?.ToString() ?? "unknown"
    )
);

// ============================================================================
// Endpoints — server API (verbatim from CybsClass.WebApi.Service Program.cs;
// the server's inline HTML home page is remapped to /server-home inside MapGets()
// so Blazor's Home.razor owns "/")
// ============================================================================
app.MapGets(); // Default pageSize: 10.
app.MapPosts();
app.MapPuts();
app.MapDeletes();

app.MapPaymentCardInfoEndpoints();

app.MapGet("/api/test", (ILogger<Program> log) =>
{
    if (loggingEnabled)
    {
        log.LogInformation("Hit /test at {Utc}", DateTimeOffset.UtcNow);
        Serilog.Log.Information("Static Serilog also wrote from /test");
    }
    return Results.Ok(new { ok = true, message = loggingEnabled ? "logged" : "logging disabled" });
});

// Diagnostic-only: reports the outbound public IP address this server actually uses when
// making outbound HTTPS calls — i.e. the IP CyberSource sees on every request from this host.
app.MapGet("/api/diag/outbound-ip", async () =>
{
    using var client = new HttpClient();
    try
    {
        var ip = (await client.GetStringAsync("https://api.ipify.org")).Trim();
        return Results.Ok(new { outboundIp = ip });
    }
    catch (Exception ex)
    {
        return Results.Ok(new { outboundIp = (string?)null, error = ex.Message });
    }
});

// Diagnostic-only: resolves apitest.cybersource.com from this server, to compare against DNS
// resolution from other origins (e.g. local machine).
app.MapGet("/api/diag/dns", async () =>
{
    try
    {
        var entry = await System.Net.Dns.GetHostEntryAsync("apitest.cybersource.com");
        return Results.Ok(new
        {
            hostName = entry.HostName,
            aliases = entry.Aliases,
            addresses = entry.AddressList.Select(a => a.ToString()).ToArray()
        });
    }
    catch (Exception ex)
    {
        return Results.Ok(new { error = ex.Message });
    }
});

// Diagnostic-only: compares this server's system clock against a reliable external reference
// clock, using the standard HTTP "Date" response header. CyberSource's HTTP Signature/JWT auth
// is time-sensitive — clock drift on the host is a classic cause of subtle auth failures.
app.MapGet("/api/diag/servertime", async () =>
{
    var serverUtcNow = DateTime.UtcNow;
    string? externalUtcRaw = null;
    DateTime? externalUtc = null;
    string? error = null;
    try
    {
        using var client = new HttpClient();
        using var response = await client.GetAsync("https://www.google.com/");
        if (response.Headers.Date.HasValue)
        {
            externalUtc = response.Headers.Date.Value.UtcDateTime;
            externalUtcRaw = response.Headers.Date.Value.ToString("o");
        }
        else
        {
            error = "No Date header in response.";
        }
    }
    catch (Exception ex)
    {
        error = ex.Message;
    }

    return Results.Ok(new
    {
        serverUtcNow = serverUtcNow.ToString("o"),
        externalUtcNow = externalUtc?.ToString("o"),
        deltaSeconds = externalUtc.HasValue ? (serverUtcNow - externalUtc.Value).TotalSeconds : (double?)null,
        error
    });
});

// Fired by the "Test CyberSource Checkout" button on the server diagnostic page ("/server-home").
// Builds a hardcoded sandbox test-card checkout DTO and runs it through the same server path as
// api/authtransaction. A success here while a client page fails points at client<->server wiring,
// not CyberSource.
app.MapPost("/api/test/checkout", async ([FromServices] CcAuthService ccAuthService) =>
{
    var testDto = new CybsClass.Cybersource.Models.DTOs.B2cCustomerDto
    {
        FirstName = "Test",
        LastName = "Checkout",
        FullName = "Test Checkout",
        Email = "test.checkout@example.com",
        Address1 = "1295 Charleston Road",
        City = "Mountain View",
        AdministrativeArea = "CA",
        PostalCode = "94043",
        Country = "US",
        Phone = "4155551234",
        AccountNumber = "4111111111111111",
        ExpMonth = "12",
        ExpYear = "2031",
        Cvv = "123",
        CardType = "001", // Visa
        TotalAmount = 10.00m,
        Currency = "USD",
        MarkedForCapture = false, // AUTH only, matching CcTransactionTypes.AUTH
        ActionTokenTypes = ["paymentInstrument"],
        ShippingFirstName = "Test",
        ShippingLastName = "Checkout",
        ShippingFullName = "Test Checkout"
    };

    var testCheckoutLogOptions = new System.Text.Json.JsonSerializerOptions { WriteIndented = true };
    Console.WriteLine($"api/test/checkout INBOUND (hardcoded) JSON: {System.Text.Json.JsonSerializer.Serialize(testDto, testCheckoutLogOptions)}");

    var jsonObject = await ccAuthService.CallForAuth(testDto);

    Console.WriteLine($"api/test/checkout OUTBOUND JSON: {jsonObject.ToJsonString(testCheckoutLogOptions)}");

    // Diagnostic-only: exposes the exact request body sent to CyberSource for this test
    // call. LastRequestSent is never read by production endpoints, so this can't leak
    // real card data through api/authtransaction or any other live transaction response.
    jsonObject["requestSent"] = CybsClass.Cybersource.Transactions.CallCyberSource.LastRequestSent;

    string? statusNode = jsonObject["status"]?.GetValue<string>();
    if (statusNode is null)
    {
        var err = new CybsClass.Cybersource.Models.BaseData.ErrorObject
        {
            Error = jsonObject["error"]?.GetValue<string>() ?? "No status returned from CyberSource",
            Message = "CyberSource did not return a parseable transaction response.",
            CybersourceJson = jsonObject["cybersourceJson"]?.GetValue<string>()
        };
        return Results.Json(err);
    }

    return Results.Json(jsonObject);
});

if (loggingEnabled)
{
    app.MapGet("/api/diag", () =>
    {
        var probe = Path.Combine(LogRoot!, "probe.txt");
        File.AppendAllText(probe, $"{DateTimeOffset.UtcNow:O} /api/diag hit{Environment.NewLine}");
        return Results.Ok(new
        {
            LogRoot,
            Expect = Path.Combine(LogRoot!, $"app-{DateTime.UtcNow:yyyy-MM-dd}.log"),
            Probe = probe
        });
    });

    app.Lifetime.ApplicationStopped.Register(Serilog.Log.CloseAndFlush);
}

app.MapGet("/exception", () => { throw new Exception(); });
app.MapGet("/timeout", () => { throw new TimeoutException(); });

app.MapGroup("/api/session").GroupSessionState();
app.MapGroup("/api/json").UtilityGroupEndPoints();
app.MapGroup("/api/semiintpos").SemiIntGroupPosEndPoints();
app.MapGroup("/api/cloudpos").GroupCloudPosEndPoints();
app.MapGroup("/api/payouts").GroupPayoutsEndPoints();
app.MapGroup("/api/paypal").GroupPayPalEndpoints();
app.MapGroup("/api/tokens").GroupTokens();
app.MapGroup("/api/unified").GroupUnifiedEndpoints();
app.MapGroup("/api/uc-config").GroupUnifiedCheckoutConfigEndpoints();
app.MapGroup("/api/invoice").GroupInvoiceEndpoints();
app.MapGroup("/api/paybylink").GroupPayByLinkEndpoints();
app.MapGroup("/api/echeck").GroupECheckEndpoints();
app.MapGroup("/api/payerauth").GroupPayerAuthEndpoints();
app.MapAuthTransResponseEndpoints();
app.MapApplePayEndpoints();
app.MapCybsLogEndpoints();
app.MapFollowOnTransResponseEndpoints();
app.MapIndividualTransactionEndpoints();
app.MapNetworkTokenInfoEndpoints();
app.MapOrderEndpoints();
app.MapOrderDetailEndpoints();
app.MapB2cCustomerEndpoints();
app.MapCategoryEndpoints();
app.MapSampleInvoiceDetailEndpoints();
app.MapPayerAuthCardSampleDatumEndpoints();
app.MapMerchantSampleDatumEndpoints();
app.MapMerchantBoardingEndpoints();
app.MapElectronicProductEndpoints();
app.MapBoardingDataEndpoints();

// ============================================================================
// Endpoints — Blazor (client). Home.razor owns "/".
// ============================================================================
app.MapRazorPages();
app.MapRazorComponents<App>().AddInteractiveServerRenderMode();
app.MapControllers();
app.UseAntiforgery();

AppErrorConfig.ErrorKeywords = new[]
{
    "INVALID_REQUEST", "EXCEPTION", "UNAUTHORIZED", "INVALID", "ERROR", "FAILED", "DECLINED",
    "exception", "unauthorized", "invalid", "error", "errors", "failed", "declined", "disabled"
};

app.Run();
