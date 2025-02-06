using System;
using System.ComponentModel;
using System.Threading.Tasks;
using CodatExtractor.DAL.Entities;
using CodatExtractor.DAL.Models;
using CodatExtractor.DAL.Services;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace CodatExtractor.AzureFunction
{
    // processes individual periods
    public class ProcessPeriod
    {
        [FunctionName("ProcessPeriod")]
        public async Task Run([ServiceBusTrigger("processperiodtopic", "ProcessPeriodSubscription", Connection = "AzureWebJobsServiceBus")]string myQueueItem, ILogger log)
        {
            var periodBody = JsonConvert.DeserializeObject<ProcessPeriodDTO>(myQueueItem);

            // initialize services
            string apikey = Environment.GetEnvironmentVariable("CodatAPIKey");
            string connectionString = Environment.GetEnvironmentVariable("ConnectionString");
            var errorLogger = new ErrorLoggingService(connectionString);
            var codatService = new CodatAPIService(apikey, errorLogger);
            var db = new COEXTRContext(connectionString);

            string taxuallyAPI = Environment.GetEnvironmentVariable("TaxuallyAPI");
            string taxuallyUser = Environment.GetEnvironmentVariable("TaxuallyUser");
            string taxuallyPass = Environment.GetEnvironmentVariable("TaxuallyPassword");
            var taxService = new TaxuallyAPIService(taxuallyAPI, taxuallyUser, taxuallyPass, errorLogger);
            var shopifyService = new ShopifyAPIService(db, errorLogger);
            string sbConnString = Environment.GetEnvironmentVariable("ServiceBusConnString");
            var serviceBusService = new ServiceBusService(sbConnString);
            var stripeAPIKey = Environment.GetEnvironmentVariable("StripeAPIKey");
            var stripeService = new StripeAPIService(stripeAPIKey, db, errorLogger);
            var runManagementService = new RunManagementService(db, codatService, taxService, errorLogger, shopifyService, serviceBusService, stripeService);

            // process period from queue
            await runManagementService.ProcessPeriod(periodBody);
        }
    }
}
