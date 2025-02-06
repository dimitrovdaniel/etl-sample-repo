using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace CodatExtractor.DAL.Models.Codat
{
    public class CompanyConnection
    {
        [JsonPropertyName("id")]
        public string Id { get; set; }

        [JsonPropertyName("integrationId")]
        public string IntegrationId { get; set; }

        [JsonPropertyName("sourceId")]
        public string SourceId { get; set; }

        [JsonPropertyName("platformName")]
        public string PlatformName { get; set; }

        [JsonPropertyName("linkUrl")]
        public string LinkUrl { get; set; }

        [JsonPropertyName("status")]
        public string Status { get; set; }

        [JsonPropertyName("lastSync")]
        public DateTime LastSync { get; set; }

        [JsonPropertyName("created")]
        public DateTime Created { get; set; }

        [JsonPropertyName("sourceType")]
        public string SourceType { get; set; }

        [JsonPropertyName("dataConnectionErrors")]
        public List<DataConnectionError> DataConnectionErrors { get; set; }

        [JsonPropertyName("connectionInfo")]
        public ConnectionInfo ConnectionInfo { get; set; }
    }

    public class CompanyConnectionResponse : BaseCodatResponse
    {
        [JsonPropertyName("results")]
        public List<CompanyConnection> Results { get; set; }
    }

}
