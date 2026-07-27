using CybsClient.Model.DBQueries;
using CybsClient.Services.DTOs;

namespace CybsClient.Services.DIServices
{
    public interface ICardService
    {
        IList<PayerAuthCardSampleDto> Cards { get; }

        event Action OnChange;

        void AddCardList(IList<PayerAuthCardSampleDto> payerAuthCardSampleDtos);
        void AddCard(PayerAuthCardSampleDto payerAuthCardSampleDto);
        void DeleteCard(PayerAuthCardSampleDto payerAuthCardSampleDto);

        public void DeleteAll();
    }
}
