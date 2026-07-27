using CybsClient.Model;
using CybsClient.Model.DBQueries;
using CybsClient.Services.DTOs;
using CybsClient.Services.Utilities;
using System.Text.Json.Nodes;
using System.Text.Json;

namespace CybsClient.Services.DIServices
{
    public class CardService : ICardService
    {
        private List<PayerAuthCardSampleDto> cards = new();
        private PayerAuthCardSampleDto card = new PayerAuthCardSampleDto();
        private int totalNumberofItems;

        public CardService()
        {
            totalNumberofItems = 0;
        }

        public CardService(int initialTotal)
        {

            totalNumberofItems = initialTotal;
        }

        public IList<PayerAuthCardSampleDto> Cards => cards;


        public event Action? OnChange;

        private void NotifyStateChanged() => OnChange?.Invoke();

        decimal? price = 0;

        public void AddCardList(IList<PayerAuthCardSampleDto> payerAuthCardSampleDtos)
        {
            cards = payerAuthCardSampleDtos.ToList();
            NotifyStateChanged();

        }
        public void AddCard(PayerAuthCardSampleDto card)
        {

            cards.Add(card);
            NotifyStateChanged();
        }
        public void DeleteCard(PayerAuthCardSampleDto card)
        {
            cards.Remove(card);
            NotifyStateChanged();
        }

        public void DeleteAll()
        {
            cards.Clear();
        }

        public async Task InitializeAsync(ISessionTransactions sessionTransactions)
        {
            // Replace with actual followOnInput if needed
            string followOnInput = "\"some-guid-or-input\"";

            var result = await CallMinAPIs.SubmitForFollowOn(followOnInput, sessionTransactions, CcTransactionTypes.SAMPLE_PA_CARDS);

            if (result?.TransactionJson is JsonArray array)
            {
                var cardList = array.Deserialize<List<PayerAuthCardSampleDto>>();
                if (cardList != null)
                    cards = cardList;
            }
        }

    }
}
