using CodatExtractor.DAL.Entities;
using CodatExtractor.DAL.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace CodatExtractor.DAL.Services.PeriodProcessors
{
    public static class PeriodProcessorFactory
    {
        public static List<IPeriodProcessor> GetPeriodProcessors(COEXTRContext context, ErrorLoggingService errorLogger, ServiceBusService serviceBusService, 
            ShopifyAPIService shopifyService, CodatAPIService codatService, StripeAPIService stripeService)
        {
            return new List<IPeriodProcessor>
            {
                new ShopifyPeriodProcessor(context, shopifyService, serviceBusService, errorLogger),
                new CodatPeriodProcessor(context, codatService, serviceBusService, errorLogger),
                new StripePeriodProcessor(context, stripeService, serviceBusService, errorLogger)
            };
        }
    }
}
