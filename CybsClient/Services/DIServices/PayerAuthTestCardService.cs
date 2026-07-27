using CybsClient.Services.DTOs;
using CybsClient.Services.Utilities;

namespace CybsClient.Services.DIServices
{
    public class PayerAuthTestCardService : IPayerAuthTestCardService
    {
        private List<PayerAuthTestCardDto> cards = new();

        public IList<PayerAuthTestCardDto> Cards => cards;

        public event Action? OnChange;

        private void NotifyStateChanged() => OnChange?.Invoke();

        public async Task InitializeAsync()
        {
            var result = await CallMinAPIs.GetPayerAuthTestCardsAsync();

            if (result.IsSuccess && result.Data is not null)
            {
                cards = result.Data;
                NotifyStateChanged();
            }
            else
            {
                Console.WriteLine($"[PayerAuthTestCardService] Failed to load PA test cards: {result.Error?.Message ?? result.Error?.Reason}");
            }
        }
    }
}
