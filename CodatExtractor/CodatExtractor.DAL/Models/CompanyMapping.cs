using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodatExtractor.DAL.Models
{
    public class CompanyMapping
    {
        public string Company { get; set; }
        public int TaxuallyCompanyID { get; set; }
        public string SourceCompanyID { get; set; }
        public bool IsNew { get; set; }
    }
}
