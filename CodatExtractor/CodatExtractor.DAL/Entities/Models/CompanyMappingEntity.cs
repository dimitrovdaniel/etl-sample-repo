using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodatExtractor.DAL.Entities.Models
{
    public class CompanyMappingEntity
    {
        public int TaxuallyCompanyID { get; set; }
        public string CodatCompanyID { get; set; }
        public string CompanyName { get; set; }
        public DateTime DateCreated { get; set; }
        public string CreatedByUser { get; set; }
    }
}
