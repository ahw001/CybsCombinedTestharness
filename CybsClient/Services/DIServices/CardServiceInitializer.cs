namespace CybsClient.Services.DIServices
{
    public class CardServiceInitializer : BackgroundService
    {
        private readonly IServiceProvider serviceProvider;
        private readonly IHostApplicationLifetime lifetime;

        public CardServiceInitializer(IServiceProvider serviceProvider, IHostApplicationLifetime lifetime)
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
            var sessionTransactions = scope.ServiceProvider.GetRequiredService<ISessionTransactions>();
            var cardService = scope.ServiceProvider.GetRequiredService<ICardService>();

            if (cardService is CardService concreteCardService)
            {
                try
                {
                    await concreteCardService.InitializeAsync(sessionTransactions);
                    Console.WriteLine("CardService initialized successfully.");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"CardService initialization failed: {ex.Message}");
                }
            }
        }

    }
}
