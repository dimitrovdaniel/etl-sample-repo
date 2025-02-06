using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace CodatExtractor.DAL.Models.Codat
{
    public class Company
    {
        [JsonPropertyName("id")]
        public string Id { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("platform")]
        public string Platform { get; set; }

        [JsonPropertyName("redirect")]
        public string Redirect { get; set; }

        [JsonPropertyName("lastSync")]
        public DateTime LastSync { get; set; }

        [JsonPropertyName("dataConnections")]
        public List<DataConnection> DataConnections { get; set; }

        [JsonPropertyName("created")]
        public DateTime Created { get; set; }

        [JsonPropertyName("createdByUserName")]
        public string CreatedByUserName { get; set; }
    }

    public class CompanyResponse : BaseCodatResponse
    {
        [JsonPropertyName("results")]
        public List<Company> Results { get; set; }
    }

}
