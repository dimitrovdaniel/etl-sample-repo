using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodatExtractor.DAL.Models.Taxually
{
    public class TaxuallyCompany
    {
        [JsonProperty("isApproved")]
        public bool IsApproved { get; set; }

        [JsonProperty("countryOfEstablishment")]
        public string CountryOfEstablishment { get; set; }

        [JsonProperty("countryCodeOfEstablishment")]
        public string CountryCodeOfEstablishment { get; set; }

        [JsonProperty("subscriptionCountries")]
        public List<int> SubscriptionCountries { get; set; }

        [JsonProperty("coCds")]
        public List<object> CoCds { get; set; }

        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("legalNameOfBusiness")]
        public string LegalNameOfBusiness { get; set; }

        [JsonProperty("countryIdOfEstablishment")]
        public int CountryIdOfEstablishment { get; set; }

        [JsonProperty("legalStatus")]
        public int LegalStatus { get; set; }

        [JsonProperty("companyStatus")]
        public int CompanyStatus { get; set; }

        [JsonProperty("profile")]
        public int Profile { get; set; }

        [JsonProperty("tenantId")]
        public string TenantId { get; set; }
    }

    public class TaxuallyCompanyResponse : StatusMessage
    {
        [JsonProperty("userId")]
        public int UserId { get; set; }

        [JsonProperty("selectedCompanyId")]
        public int? SelectedCompanyId { get; set; }

        [JsonProperty("selectedTenantId")]
        public string SelectedTenantId { get; set; }

        [JsonProperty("firstName")]
        public string FirstName { get; set; }

        [JsonProperty("lastName")]
        public string LastName { get; set; }

        [JsonProperty("email")]
        public string Email { get; set; }

        [JsonProperty("language")]
        public string Language { get; set; }

        [JsonProperty("isAdmin")]
        public bool IsAdmin { get; set; }

        [JsonProperty("isClientAdmin")]
        public bool IsClientAdmin { get; set; }

        [JsonProperty("isTechnicalAdmin")]
        public bool IsTechnicalAdmin { get; set; }

        [JsonProperty("companies")]
        public List<TaxuallyCompany> Companies { get; set; }

        [JsonProperty("tenants")]
        public List<Tenant> Tenants { get; set; }

        [JsonProperty("availableFeatures")]
        public List<int> AvailableFeatures { get; set; }
    }

    public class Tenant
    {
        [JsonProperty("guid")]
        public string Guid { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }
    }
}
