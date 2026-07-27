using System.Text.Json.Nodes;

namespace CybsClient.Services.DIServices
{
    public interface ITransactionJson
    {
        IList<JsonNode> Transactions { get; }

        event Action? OnChange;

        public string TransactionJsonInfo();

        void AddTrans(JsonNode trans);
        void DeleteTrans(JsonNode trans);
    }
}