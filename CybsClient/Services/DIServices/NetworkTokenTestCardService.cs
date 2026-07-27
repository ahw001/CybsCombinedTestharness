using CybsClient.Model;
using CybsClient.Services.Utilities;

namespace CybsClient.Services.DIServices
{
    public class NetworkTokenTestCardService : INetworkTokenTestCardService
    {
        private List<NetworkTokenTestCardDto> cards = new();

        public IList<NetworkTokenTestCardDto> Cards => cards;

        public event Action? OnChange;

        private void NotifyStateChanged() => OnChange?.Invoke();

        public async Task InitializeAsync()
        {
            var result = await CallMinAPIs.GetNetworkTokenTestCardsAsync();

            if (result.IsSuccess && result.Data is not null)
            {
                cards = result.Data;
                NotifyStateChanged();
            }
            else
            {
                Console.WriteLine($"[NetworkTokenTestCardService] Failed to load NT test cards: {result.Error?.Message ?? result.Error?.Reason}");
            }
        }
    }
}
