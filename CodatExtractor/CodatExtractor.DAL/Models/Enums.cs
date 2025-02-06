using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodatExtractor.DAL.Models
{
    public enum OriginSource
    {
        ExtractorApp,
        CodatAPI,
        TaxuallyAPI,
        ShopifyAPI,
        StripeAPI
    }

    public enum ProcessingOutcome
    {
        ProcessedWithData,
        ProcessedWithoutData,
        ProcessingToContinue,
        ProcessingError
    }
}
