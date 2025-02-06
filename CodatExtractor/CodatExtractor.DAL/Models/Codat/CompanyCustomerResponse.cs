using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodatExtractor.DAL.Models.Codat
{
    public class CompanyCustomer : StatusMessage
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("customerName")]
        public string CustomerName { get; set; }

        [JsonProperty("emailAddress")]
        public string EmailAddress { get; set; }

        [JsonProperty("defaultCurrency")]
        public string DefaultCurrency { get; set; }

        [JsonProperty("phone")]
        public string Phone { get; set; }

        [JsonProperty("addresses")]
        public List<Address> Addresses { get; set; }

        [JsonProperty("note")]
        public string Note { get; set; }

        [JsonProperty("createdDate")]
        public DateTime CreatedDate { get; set; }

        [JsonProperty("modifiedDate")]
        public DateTime ModifiedDate { get; set; }

        [JsonProperty("sourceModifiedDate")]
        public DateTime SourceModifiedDate { get; set; }
    }

    public class CompanyCustomerResponse : BaseCodatResponse
    {
        [JsonProperty("results")]
        public List<CompanyCustomer> Results { get; set; }
    }
}
