using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodatExtractor.DAL.Entities.Models
{
    public class RunCompanyPeriodEntity
    {
        public string RunTimestamp { get; set; }
        public int TaxuallyCompanyID { get; set; }
        public string CompanyName { get; set; }
        public string PeriodID { get; set; }
        public string PeriodDate { get; set; }
        public int PeriodStatus { get; set; }
        public int TotalOrdersPulled { get; set; }
        public int? TotalInvoicesPulled { get; set; }
        public int? TotalBillsPulled { get; set; }
        public int? TotalBillCreditNotesPulled { get; set; }
        public int? TotalCreditNotesPulled { get; set; }
        public bool? IsPeriodProcessed { get; set; }
        public DateTime? ProcessingStartedAt { get; set; }
        public string LastDataID { get; set; }
        public virtual RunEntity Run { get; set; }
    }
}
