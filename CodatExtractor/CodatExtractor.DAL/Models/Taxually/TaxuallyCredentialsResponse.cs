using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodatExtractor.DAL.Models.Taxually
{
    public class TaxuallyCredentialsResponse
    {
        public List<TaxuallyCred> Credentials { get; set; }
    }

    public class TaxuallyCred
    {
        public bool IsNew { get; set; }
        public string User { get; set; }
        public string Password { get; set; }
    }
}
