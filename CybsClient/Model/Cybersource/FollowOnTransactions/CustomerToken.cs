using CybsClient.Model.Cybersource.BaseData;
using System.Text.Json.Serialization;

namespace CybsClient.Model.Cybersource.FollowOnTransactions
{
    public class CustomerToken
    {
        [JsonPropertyName("_links")]
        public Links? Links { get; set; }

        [JsonPropertyName("id")]
        public string? Id { get; set; }

        [JsonPropertyName("buyerInformation")]
        public BuyerInformation? BuyerInformation { get; set; }

        [JsonPropertyName("metadata")]
        public Metadata? Metadata { get; set; }
    }
}
