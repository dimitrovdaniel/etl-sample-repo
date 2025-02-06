using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodatExtractor.DAL.Models.Taxually
{
    public class TaxTokenResponse
    {
        [JsonProperty("expiration")]
        public int Expiration { get; set; }
        [JsonProperty("token")]
        public string Token { get; set; }
    }
}
