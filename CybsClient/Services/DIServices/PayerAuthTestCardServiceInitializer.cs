namespace CybsClient.Services.DIServices
{
    public class PayerAuthTestCardServiceInitializer : BackgroundService
    {
        private readonly IServiceProvider serviceProvider;
        private readonly IHostApplicationLifetime lifetime;

        public PayerAuthTestCardServiceInitializer(IServiceProvider serviceProvider, IHostApplicationLifetime lifetime)
        {
            this.serviceProvider = serviceProvider;
            this.lifetime = lifetime;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            // Combined single-host: the API this initializer calls lives in this same process,
            // so wait until Kestrel is actually listening before making the loopback HTTP call.
            var started = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
            using var reg = lifetime.ApplicationStarted.Register(() => started.TrySetResult());
            await Task.WhenAny(started.Task, Task.Delay(Timeout.Infinite, stoppingToken));
            if (stoppingToken.IsCancellationRequested) return;

            using var scope = serviceProvider.CreateScope();
            var payerAuthTestCardService = scope.ServiceProvider.GetRequiredService<IPayerAuthTestCardService>();

            try
            {
                await payerAuthTestCardService.InitializeAsync();
                Console.WriteLine("PayerAuthTestCardService initialized successfully.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"PayerAuthTestCardService initialization failed: {ex.Message}");
            }
        }
    }
}
