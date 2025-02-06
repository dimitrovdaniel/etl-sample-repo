using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodatExtractor.DAL.Entities.Models
{
    public class RunCSVEntity
    {
        public string RunTimestamp { get; set; }
        public string CSVText { get; set; }
        public int TaxuallyCompanyID { get; set; }
        public string CompanyName { get; set; }
        public string PeriodRange { get; set; }
        public string PeriodID { get; set; }
        public bool IsUploadedToTaxually { get; set; }
        public bool IsEnqueuedOnTaxually { get; set; }
        public virtual RunEntity Run { get; set; }
    }
}
