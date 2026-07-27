using CybsClient.Model.DBQueries;

namespace CybsClient.Services.DIServices
{
    public interface ICartService
    {
        IList<DBProduct> Cart { get; }
        decimal? Total { get; }
        public Guid Value { get; }

        event Action OnChange;
        void AddProduct(DBProduct product);
        void DeleteProduct(DBProduct product);
        void SetTotal(decimal? total);

        public void DeleteAll();

    }
}

