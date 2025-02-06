using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodatExtractor.DAL.Models.Taxually
{
    public class TaxuallyPeriodBatchRequest
    {
        [JsonProperty("dataPeriodId")]
        public string DataPeriodID { get; set; }
        [JsonProperty("uploadBatchId")]
        public string UploadBatchID { get; set; }
    }
}
