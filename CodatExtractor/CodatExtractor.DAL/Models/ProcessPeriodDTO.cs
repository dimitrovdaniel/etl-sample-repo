using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodatExtractor.DAL.Models
{
    public class ProcessPeriodDTO
    {
        public string RunTimestamp { get; set; }
        public string PeriodId { get; set; }
        public int CompanyId { get; set; }
        public string CompanyName { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public Guid CompanyBatchId { get; set; }
        public string TaxuallyUser { get; set; }
        public string TaxuallyPassword { get; set; }
        public string CodatCompanyId { get; set; }
        public string StripeAccountId { get; set; }
        
        // to split period processing into separate runs to avoid timeouts
        public string StartAtDataID { get; set; }
        public string StartAtOriginID { get; set; }
        public OriginSource? ContinueFromSource { get; set; }
    }
}
