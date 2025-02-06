using CodatExtractor.DAL.Entities;
using CodatExtractor.DAL.Models;
using CodatExtractor.DAL.Models.Shopify;
using CodatExtractor.DAL.Models.Stripe;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodatExtractor.DAL.Services.PeriodProcessors
{
    public class StripePeriodProcessor : IPeriodProcessor
    {
        private COEXTRContext _context;
        private StripeAPIService _stripeService;
        private ServiceBusService _serviceBusService;
        private ErrorLoggingService _errorLogger;

        public OriginSource OriginSource { get; set; }

        public StripePeriodProcessor(COEXTRContext context, StripeAPIService stripeService, ServiceBusService serviceBusService, ErrorLoggingService errorLogger)
        {
            _context = context;
            _stripeService = stripeService;
            _serviceBusService = serviceBusService;
            _errorLogger = errorLogger;

            OriginSource = OriginSource.StripeAPI;
        }

        public async Task<ProcessingOutcome> ProcessPeriod(ProcessPeriodDTO periodInfo, string tempCSVFilePath)
        {
            if (periodInfo.StripeAccountId == null) // not a Stripe run, skip
                return ProcessingOutcome.ProcessedWithoutData;

            bool periodHasData = false;
            var dbPeriod = _context.RunCompanyPeriods.FirstOrDefault(x => x.RunTimestamp == periodInfo.RunTimestamp && x.PeriodID == periodInfo.PeriodId);

            string lastId = periodInfo.StartAtDataID;
            periodInfo.StartAtDataID = null; // start ID consumed, set to null

            var stripeInvoices = new StripeInvoicesResponse();

            int totalOrderCount = dbPeriod.TotalOrdersPulled;
            int currentInvoiceCount = 0;

            do
            {
                // get stripe invoices
                stripeInvoices = await _stripeService.GetInvoices(periodInfo.StripeAccountId, periodInfo.StartDate, periodInfo.EndDate, lastId);
                lastId = stripeInvoices.Results.Select(x => x.Id).LastOrDefault();
                currentInvoiceCount += stripeInvoices.Results.Count();
                totalOrderCount += stripeInvoices.Results.Count();

                if (dbPeriod != null && stripeInvoices.Results != null)
                    dbPeriod.TotalOrdersPulled = totalOrderCount;

                if (stripeInvoices.Results.Count() > 0)
                    periodHasData = true;

                // live update period data
                await _context.SaveChangesAsync();

                // build CSV from Stripe data
                string csvDataHeader = "BusinessPartnerId,BusinessPartnerName,BusinessPartnerCountry,BusinessPartnerVatNumber,PostalCode,InvoiceId,DocumentNo,Description,Description2,ClientTaxCode," +
                            "SaleArrivalCountry,CurrencyCode,TransactionDate,GrossAmount,NetAmount,VatAmount,SaleDepartureCountry,SalesChannel,Quantity,VatRate,SKU,TransactionType,OriginalInvoiceId,DataSource";

                // create file if needed
                bool fileExists = File.Exists(tempCSVFilePath);
                StreamWriter fileWriter = new StreamWriter(tempCSVFilePath, true);
                if (!fileExists)
                    fileWriter.WriteLine(csvDataHeader);

                var companyData = new List<CSVData>();

                // build invoices CSV
                foreach (var invoice in stripeInvoices.Results)
                {
                    companyData.Add(new CSVData
                    {
                        InvoiceId = invoice.Id,
                        BusinessPartnerCountry = invoice.CustomerAddress?.Country,
                        BusinessPartnerId = invoice.Customer?.Id,
                        BusinessPartnerName = invoice.Customer?.Name,
                        CountryType = "",
                        PostalCode = invoice.CustomerAddress?.PostalCode,
                        SaleArrivalCountry = "",
                        CurrencyCode = invoice.Currency,
                        TransactionDate = invoice.StatusTransitions?.FinalizedAt ?? invoice.DueDate ?? DateTime.MinValue,
                        DocumentNo = invoice.Number,
                        Description = invoice.Description,
                        ClientTaxCode = "",
                        GrossAmount = invoice.Total,
                        NetAmount = invoice.TotalExcludingTax,
                        VatAmount = invoice.TotalTaxAmounts?.Sum(x => (long?)x.Amount) ?? 0,
                        Quantity = null,
                        VatRate = null,
                        SKU = "",
                        TransactionType = "Sale",
                        DataSource = "Stripe"
                    });
                }

                foreach (var data in companyData)
                {
                    string row = $"\"{data.BusinessPartnerId}\",\"{data.BusinessPartnerName}\",\"{data.BusinessPartnerCountry}\",\"\",\"{data.PostalCode}\",\"{data.InvoiceId}\"," +
                        $"\"{data.DocumentNo}\",\"{data.Description}\",\"{data.Description2}\",\"{data.ClientTaxCode}\",\"{data.SaleArrivalCountry}\",\"{data.CurrencyCode}\",\"{data.TransactionDate.ToString("M/d/yyyy")}\"," +
                        $"\"{data.GrossAmount}\",\"{data.NetAmount}\",\"{data.VatAmount}\",\"\",\"\",\"{data.Quantity}\",\"{data.VatRate}\",\"{data.SKU}\"," +
                        $"\"{data.TransactionType}\",\"{data.OriginalInvoiceId}\",\"{data.DataSource}\"";

                    // write on each iteration to avoid memory issues
                    fileWriter.WriteLine(row);
                }

                fileWriter.Close();

                // every 10k orders will be processed on a separate run
                if (currentInvoiceCount >= 10000)
                {
                    _serviceBusService.InitServiceBus();

                    periodInfo.StartAtDataID = lastId;

                    // send to the AF for a new instance to process
                    await _serviceBusService.SendMessage(JsonConvert.SerializeObject(periodInfo));

                    await _serviceBusService.DisposeServiceBus();
                    return ProcessingOutcome.ProcessingToContinue;
                }
            }
            while (stripeInvoices.Results.Count() >= 100);

            return periodHasData ? ProcessingOutcome.ProcessedWithData : ProcessingOutcome.ProcessedWithoutData;
        }
    }
}
