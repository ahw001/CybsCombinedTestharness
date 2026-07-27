namespace CybsClient.Services.DIServices
{
    /// <summary>
    /// Which kind of test the user selected at the start of a Flex Microform checkout flow.
    /// Determines which card-source table/service the card-entry step later shows:
    /// Standard -> PaymentCardSampleData (ICardService), NetworkTokenTest -> NetworkTokenTestCard
    /// (INetworkTokenTestCardService, CyberSource's official NT/T4T test matrix), PayerAuthentication
    /// -> PayerAuthCardSampleData (IPayerAuthTestCardService, full 3DS 2.x test-case matrix).
    /// </summary>
    public enum TransactionMode
    {
        Standard,
        NetworkTokenTest,
        PayerAuthentication,
    }

    public interface ITransactionModeState
    {
        TransactionMode Mode { get; }

        event Action? OnChange;

        void SetMode(TransactionMode mode);
    }
}
