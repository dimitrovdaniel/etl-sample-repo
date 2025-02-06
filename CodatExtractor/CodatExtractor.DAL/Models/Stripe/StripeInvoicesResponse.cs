using Newtonsoft.Json;
using Stripe;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodatExtractor.DAL.Models.Stripe
{
    public class StripeInvoicesResponse : StatusMessage
    {
        public StripeList<Invoice> Results { get; set; }
    }

}
