using CybsClient.Model.Cybersource.Transactions;

namespace CybsClient.Services.DIServices
{
    public interface ISessionTransactions
    {
        IList<SessionTransJson> Transactions { get; }

        event Action? OnChange;

        void AddTrans(SessionTransJson trans);
        void DeleteAll();
        void DeleteTrans(SessionTransJson trans);
        string TransactionJsonInfo();
    }
}