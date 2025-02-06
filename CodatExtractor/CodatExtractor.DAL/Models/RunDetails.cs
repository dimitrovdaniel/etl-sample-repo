using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodatExtractor.DAL.Models
{
    public class RunDetailsResponse
    {
        public List<CompanyRunResults> CompanyRunResults { get; set; }
        public List<RunDetails> Last10Results { get; set; }
    }

    public class CompanyRunResults
    {
        public string CompanyName { get; set; }
        public string LastActiveRun { get; set; }
        public string Period { get; set; }
        public string Status { get; set; }
        public string ItemsPulled { get; set; }
    }

    public class RunDetails
    {
        public string RunTimestamp { get; set; }
        public bool IsCompleted { get; set; }
        public bool InProgress { get; set; }
        public bool WasSuccessful { get; set; }
        public bool WasUserInvoked { get; set; }
        public string InvokedByUser { get; set; }
        public List<RunDetailsRecord> RunDetailsRecords { get; set; }
        public List<RunErrorRecord> RunErrors { get; set; }
    }

    public class RunErrorRecord
    {
        public DateTime DateCreated { get; set; }
        public string Source { get; set; }
        public string ErrorMessage { get; set; }
        public string RawException { get; set; }
    }

    public class RunDetailsRecord
    {
        public bool IsPeriodProcessed { get; set; }
        public string CSVText { get; set; }
        public int TaxuallyCompanyID { get; set; }
        public string CompanyName { get; set; }
        public string PeriodRange { get; set; }
        public bool IsUploadedToTaxually { get; set; }
        public bool IsEnqueuedOnTaxually { get; set; }
        public int PeriodStatus { get; set; }
        public int TotalOrdersPulled { get; set; }
        public int TotalInvoicesPulled { get; set; }
        public int TotalBillsPulled { get; set; }
        public int TotalBillCreditNotesPulled { get; set; }
        public int TotalCreditNotesPulled { get; set; }
        public DateTime? ProcessingStartedAt { get; set; }
    }
}
