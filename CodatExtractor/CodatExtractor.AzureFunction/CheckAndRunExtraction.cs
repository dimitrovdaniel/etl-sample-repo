using System;
using System.Threading.Tasks;
using CodatExtractor.DAL.Entities;
using CodatExtractor.DAL.Services;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;

namespace CodatExtractor.AzureFunction
{
    // checks schedule for upcoming runs and executes recorded runs
    public class CheckAndRunExtraction
    {
        [FunctionName("CheckAndRunExtraction")]
        public async Task Run([TimerTrigger("0 * * * * *")]TimerInfo myTimer, ILogger log)
        {
            log.LogInformation($"C# Timer trigger function executed at: {DateTime.Now}");

            // initialize all services
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

            // check scheduled settings and create run records
            await runManagementService.CheckAndInsertScheduledRuns();
            // perform runs
            await runManagementService.CheckAndRunLogic();
        }
    }
}
