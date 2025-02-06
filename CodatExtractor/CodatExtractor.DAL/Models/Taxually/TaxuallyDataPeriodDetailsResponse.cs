using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodatExtractor.DAL.Models.Taxually
{
    public class TaxuallyDataPeriodDetailsResponse : StatusMessage
    {
        [JsonProperty("dataPeriodId")]
        public int DataPeriodId { get; set; }

        [JsonProperty("start")]
        public DateTime Start { get; set; }

        [JsonProperty("end")]
        public DateTime End { get; set; }

        [JsonProperty("frequency")]
        public int Frequency { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("status")]
        public int Status { get; set; }

        [JsonProperty("canRestart")]
        public bool CanRestart { get; set; }

        [JsonProperty("sftpAvailable")]
        public bool SftpAvailable { get; set; }

        [JsonProperty("fineTuningOptions")]
        public int FineTuningOptions { get; set; }

        [JsonProperty("uploadedFiles")]
        public List<UploadedFile> UploadedFiles { get; set; }

        [JsonProperty("hasPendingRules")]
        public bool HasPendingRules { get; set; }

        [JsonProperty("processingError")]
        public object ProcessingError { get; set; }
    }

    public class UploadedFile
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("transactionFileSource")]
        public int TransactionFileSource { get; set; }

        [JsonProperty("status")]
        public int Status { get; set; }

        [JsonProperty("hasWarnings")]
        public bool HasWarnings { get; set; }
    }
}
