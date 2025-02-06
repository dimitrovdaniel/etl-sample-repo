using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace CodatExtractor.DAL.Models.Codat
{
    public class BaseCodatResponse : StatusMessage
    {
        [JsonPropertyName("pageNumber")]
        public int PageNumber { get; set; }

        [JsonPropertyName("pageSize")]
        public int PageSize { get; set; }

        [JsonPropertyName("totalResults")]
        public int TotalResults { get; set; }

        [JsonPropertyName("_links")]
        public Links Links { get; set; }
    }

    public class ConnectionInfo
    {
        [JsonPropertyName("additionalProp1")]
        public string AdditionalProp1 { get; set; }

        [JsonPropertyName("additionalProp2")]
        public string AdditionalProp2 { get; set; }

        [JsonPropertyName("additionalProp3")]
        public string AdditionalProp3 { get; set; }
    }

    public class Current
    {
        [JsonPropertyName("href")]
        public string Href { get; set; }
    }

    public class DataConnection
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

    public class DataConnectionError
    {
        [JsonPropertyName("statusCode")]
        public string StatusCode { get; set; }

        [JsonPropertyName("statusText")]
        public string StatusText { get; set; }

        [JsonPropertyName("errorMessage")]
        public string ErrorMessage { get; set; }

        [JsonPropertyName("erroredOnUtc")]
        public DateTime ErroredOnUtc { get; set; }
    }

    public class Links
    {
        [JsonPropertyName("self")]
        public Self Self { get; set; }

        [JsonPropertyName("current")]
        public Current Current { get; set; }

        [JsonPropertyName("next")]
        public Next Next { get; set; }

        [JsonPropertyName("previous")]
        public Previous Previous { get; set; }
    }

    public class Next
    {
        [JsonPropertyName("href")]
        public string Href { get; set; }
    }

    public class Previous
    {
        [JsonPropertyName("href")]
        public string Href { get; set; }
    }

    public class Self
    {
        [JsonPropertyName("href")]
        public string Href { get; set; }
    }


    public class SupplementalData
    {
        [JsonProperty("platformKey")]
        public string PlatformKey { get; set; }

        [JsonProperty("content")]
        public List<Content> Content { get; set; }
    }
    public class Metadata
    {
        [JsonProperty("isDeleted")]
        public bool IsDeleted { get; set; }
    }
    public class Content
    {
        [JsonProperty("dataSource")]
        public string DataSource { get; set; }

        [JsonProperty("data")]
        public string Data { get; set; }
    }
    public class Address
    {
        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("line1")]
        public string Line1 { get; set; }

        [JsonProperty("line2")]
        public string Line2 { get; set; }

        [JsonProperty("city")]
        public string City { get; set; }

        [JsonProperty("region")]
        public string Region { get; set; }

        [JsonProperty("country")]
        public string Country { get; set; }

        [JsonProperty("postalCode")]
        public string PostalCode { get; set; }
    }

    public class SupplierRef
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("supplierName")]
        public string SupplierName { get; set; }
    }
    public class TaxRateRef
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("effectiveTaxRate")]
        public decimal EffectiveTaxRate { get; set; }
    }


}
