using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodatExtractor.DAL.Models.Codat
{
    public class CompanyTransaction
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("totalAmount")]
        public decimal TotalAmount { get; set; }

        [JsonProperty("currency")]
        public string Currency { get; set; }

        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("subType")]
        public string SubType { get; set; }

        [JsonProperty("transactionSourceRef")]
        public TransactionSourceRef TransactionSourceRef { get; set; }

        [JsonProperty("createdDate")]
        public DateTime CreatedDate { get; set; }

        [JsonProperty("modifiedDate")]
        public DateTime ModifiedDate { get; set; }

        [JsonProperty("sourceModifiedDate")]
        public DateTime SourceModifiedDate { get; set; }
    }

    public class CompanyTransactionResponse : BaseCodatResponse
    {
        [JsonProperty("results")]
        public List<CompanyTransaction> Results { get; set; }
    }

    public class TransactionSourceRef
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("type")]
        public string Type { get; set; }
    }

}
