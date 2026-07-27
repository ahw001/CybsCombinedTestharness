using CybsClient.Model;

namespace CybsClient.Services.DIServices
{
    /// <summary>
    /// The official CyberSource Network Token / Token-for-Token sandbox test-card matrix
    /// (NetworkTokenTestCard table, sourced verbatim from CyberSource's published test-cards
    /// page). Deliberately separate from ICardService/CardService, which serves the older,
    /// generic PaymentCardSampleData table — that table's values do not match CyberSource's
    /// current published NT test cards (e.g. Visa expiration), so NT-specific test flows must
    /// use this service instead.
    /// </summary>
    public interface INetworkTokenTestCardService
    {
        IList<NetworkTokenTestCardDto> Cards { get; }

        event Action? OnChange;

        Task InitializeAsync();
    }
}
