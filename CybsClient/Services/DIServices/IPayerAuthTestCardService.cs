using CybsClient.Services.DTOs;

namespace CybsClient.Services.DIServices
{
    /// <summary>
    /// Full CyberSource 3-D Secure 2.x sandbox test-card matrix (PayerAuthCardSampleData table).
    /// Deliberately separate from ICardService/CardService (which serves the unrelated
    /// PaymentCardSampleData NT/T4T table) so PA-test-card selection is never mixed with the
    /// standard/Network-Token card list.
    /// </summary>
    public interface IPayerAuthTestCardService
    {
        IList<PayerAuthTestCardDto> Cards { get; }

        event Action? OnChange;

        Task InitializeAsync();
    }
}
