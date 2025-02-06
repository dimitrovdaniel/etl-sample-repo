using CodatExtractor.DAL.Entities;
using CodatExtractor.DAL.Entities.Models;
using CodatExtractor.DAL.Models;
using CodatExtractor.DAL.Models.Shopify;
using CodatExtractor.DAL.Models.Stripe;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Stripe;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodatExtractor.DAL.Services
{
    // gets and processes data from Stripe
    public class StripeAPIService
    {
        private COEXTRContext _context;
        private ErrorLoggingService _errorLogger;

        public StripeAPIService(string apiKey, COEXTRContext context, ErrorLoggingService errorLogger)
        {
            _context = context;
            _errorLogger = errorLogger;

            // specify Stripe API key
            StripeConfiguration.ApiKey = apiKey;
        }

        // get all connected accounts
        public StripeList<Account> GetAccounts()
        {
            var options = new AccountListOptions
            {
                Limit = 100,
            };
            var service = new AccountService();
            StripeList<Account> accounts = service.List(
              options);

            return accounts;
        }

        // get invoices for account
        public async Task<StripeInvoicesResponse> GetInvoices(string stripeAccountId, DateTime startDate, DateTime endDate, string lastId = null)
        {
            try
            {
                // build invoice request
                var options = new InvoiceListOptions
                {
                    Limit = 100,
                    Created = new DateRangeOptions { GreaterThanOrEqual = startDate, LessThanOrEqual = endDate },
                    StartingAfter = lastId,
                };
                var service = new InvoiceService();
                var ro = new RequestOptions();
                ro.StripeAccount = stripeAccountId;
                StripeList<Invoice> invoices = await service.ListAsync(options);

                return new StripeInvoicesResponse
                {
                    Results = invoices
                };
            }
            catch (Exception ex)
            {
                // log error
                await _errorLogger.LogError(ex, OriginSource.StripeAPI, DateTime.UtcNow.ToString("yyyy-MM-dd-HH-mm-ss"));

                return new StripeInvoicesResponse { Error = true, Message = ex.Message };
            }
        }
    }
}
