using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodatExtractor.DAL.Models.Taxually
{
    public class TaxuallyCompanyWithPeriods
    {
        public int CompanyId { get; set; }
        public string CompanyName { get; set; }
        public string TaxuallyUser { get; set; }
        public string TaxuallyPassword { get; set; }
        public Guid BatchId { get; set; }
        public List<TaxuallyPeriod> Periods { get; set; }
    }

    public class TaxuallyPeriod
    {
        public string Id { get; set; }
        public string Date { get; set; }
        public string CSV { get; set; }
        public bool IsProcessed { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
    }
}
