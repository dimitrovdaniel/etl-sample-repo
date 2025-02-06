using CodatExtractor.DAL.Entities;
using CodatExtractor.DAL.Models;
using CodatExtractor.DAL.Models.Shopify;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodatExtractor.DAL.Services.PeriodProcessors
{
    public class ShopifyPeriodProcessor : IPeriodProcessor
    {
        private COEXTRContext _context;
        private ShopifyAPIService _shopifyService;
        private ServiceBusService _serviceBusService;
        private ErrorLoggingService _errorLogger;

        public OriginSource OriginSource { get; set; }

        public ShopifyPeriodProcessor(COEXTRContext context, ShopifyAPIService shopifyService, ServiceBusService serviceBusService, ErrorLoggingService errorLogger)
        {
            _context = context;
            _shopifyService = shopifyService;
            _serviceBusService = serviceBusService;
            _errorLogger = errorLogger;

            OriginSource = OriginSource.ShopifyAPI;
        }

        public async Task<ProcessingOutcome> ProcessPeriod(ProcessPeriodDTO periodInfo, string tempCSVFilePath)
        {
            var shops = _context.ShopifyMappings.Where(x => x.TaxuallyCompanyID == periodInfo.CompanyId)
                            .Select(x => x.ShopifyShop).ToList(); // handle multi companies to one ID
            var dbPeriod = _context.RunCompanyPeriods.FirstOrDefault(x => x.RunTimestamp == periodInfo.RunTimestamp && x.PeriodID == periodInfo.PeriodId);
            bool periodHasData = false;
            int totalOrderCount = dbPeriod.TotalOrdersPulled;
            int currentRunOrderCount = 0;

            // add small random delay to avoid Shopify API limit exception
            Random randomizer = new Random();
            int delaySeconds = randomizer.Next(500, 2000);
            Thread.Sleep(delaySeconds);

            // process Shopify period data
            foreach (var shop in shops)
            {
                // skip until reached the target Shopify Shop ID
                if ((periodInfo.StartAtOriginID != null && periodInfo.StartAtOriginID != shop.ShopID))
                    continue;

                long lastId = periodInfo.StartAtDataID != null ? long.Parse(periodInfo.StartAtDataID) : 0;
                periodInfo.StartAtDataID = null; // start id consumed, set to null

                var ordersBatch = new List<Order>();
                int orderCountPreFilter = 0;

                do
                {
                    // get Shopify orders
                    var orders = await _shopifyService.GetOrders(shop.ShopID, shop.DataAccessToken,
                        periodInfo.StartDate.AddDays(-3).ToString("yyyy-MM-dd"), // offset by 3 days to take into account timezones 
                        periodInfo.EndDate.AddDays(3).ToString("yyyy-MM-dd"),
                        lastId);

                    if (orders.Orders == null)
                    {
                        // invalidated/uninstalled company
                        if (orders.Message != null && (orders.Message.ToLower().Contains("invalid api key") || orders.Message.ToLower().Contains("access token")))
                        {
                            _context.ShopifyMappings.RemoveRange(shop.ShopifyMappings.ToList());
                            _context.ShopifyShops.Remove(shop);
                            await _context.SaveChangesAsync();

                            await _errorLogger.LogError(new Exception(orders.Message), OriginSource.TaxuallyAPI, periodInfo.RunTimestamp, $"Company mapping ({periodInfo.CompanyId}) {periodInfo.CompanyName} has been removed due to Shopify App being uninstalled.");
                            break;
                        }

                        periodHasData = false; // do not upload if errors
                        await _errorLogger.LogError(new Exception(orders.Message), OriginSource.TaxuallyAPI, periodInfo.RunTimestamp, $"Failed to get Shopify orders from API for company ({periodInfo.CompanyId}) {periodInfo.CompanyName} and period {periodInfo.StartDate} - {periodInfo.EndDate}.");
                        break;
                    }

                    orderCountPreFilter = orders.Orders.Count;
                    // count prefilter to avoid timeouts on periods with lots of skipped orders
                    currentRunOrderCount += orderCountPreFilter;

                    ordersBatch = orders.Orders
                            .Where(x => x.CreatedAt.Date >= periodInfo.StartDate
                                && x.CreatedAt.Date <= periodInfo.EndDate)
                            .ToList();

                    if (ordersBatch.Count > 0)
                        periodHasData = true;

                    // record total order count and last id
                    totalOrderCount += ordersBatch.Count;
                    lastId = orders.Orders.Select(x => x.Id).LastOrDefault();

                    if (dbPeriod != null) // update DB with progress
                    {
                        dbPeriod.TotalOrdersPulled = totalOrderCount;
                        await _context.SaveChangesAsync();
                    }

                    // build CSV
                    string csvDataHeader = "BusinessPartnerId,BusinessPartnerName,BusinessPartnerCountry,BusinessPartnerVatNumber,PostalCode,InvoiceId,DocumentNo,Description,Description2,ClientTaxCode," +
                        "SaleArrivalCountry,CurrencyCode,TransactionDate,GrossAmount,NetAmount,VatAmount,SaleDepartureCountry,SalesChannel,Quantity,VatRate,SKU,TransactionType,OriginalInvoiceId,DataSource," +
                        "Gateway,StoreRef";

                    // create file if needed
                    bool fileExists = File.Exists(tempCSVFilePath);
                    StreamWriter fileWriter = new StreamWriter(tempCSVFilePath, true);
                    if (!fileExists)
                        fileWriter.WriteLine(csvDataHeader);

                    var companyData = new List<CSVData>();

                    // process orders
                    foreach (var order in ordersBatch)
                    {
                        var grossAmount = order.CurrentTotalPriceSet?.PresentmentMoney?.Amount != null ?
                            decimal.Parse(order.CurrentTotalPriceSet?.PresentmentMoney?.Amount) : 0;

                        var vatAmount = order.CurrentTotalTaxSet?.PresentmentMoney?.Amount != null ?
                            decimal.Parse(order.CurrentTotalTaxSet?.PresentmentMoney?.Amount) : 0;

                        companyData.Add(new CSVData
                        {
                            InvoiceId = order.Id.ToString(),
                            BusinessPartnerCountry = order.BillingAddress?.CountryCode,
                            BusinessPartnerId = order.Customer?.Id.ToString(),
                            BusinessPartnerName = "",
                            CountryType = "",
                            PostalCode = order.ShippingAddress?.Zip,
                            SaleArrivalCountry = order.ShippingAddress?.CountryCode,
                            CurrencyCode = order.PresentmentCurrency,
                            TransactionDate = order.ProcessedAt,
                            DocumentNo = order.OrderNumber.ToString(),
                            Description = "",
                            ClientTaxCode = "",
                            GrossAmount = grossAmount,
                            NetAmount = null,
                            VatAmount = vatAmount,
                            Quantity = null,
                            VatRate = null,
                            SKU = "",
                            TransactionType = "Sale",
                            DataSource = "Shopify",
                            Gateway = order.Gateway
                        });
                    }

                    foreach (var data in companyData)
                    {
                        string row = $"\"{data.BusinessPartnerId}\",\"{data.BusinessPartnerName}\",\"{data.BusinessPartnerCountry}\",\"\",\"{data.PostalCode}\",\"{data.InvoiceId}\"," +
                            $"\"{data.DocumentNo}\",\"{data.Description}\",\"{data.Description2}\",\"{data.ClientTaxCode}\",\"{data.SaleArrivalCountry}\",\"{data.CurrencyCode}\",\"{data.TransactionDate.ToString("M/d/yyyy")}\"," +
                            $"\"{data.GrossAmount}\",\"{data.NetAmount}\",\"{data.VatAmount}\",\"\",\"\",\"{data.Quantity}\",\"{data.VatRate}\",\"{data.SKU}\"," +
                            $"\"{data.TransactionType}\",\"{data.OriginalInvoiceId}\",\"{data.DataSource}\",\"{data.Gateway}\",\"{shop.ShopID}\"";

                        // write data on each loop to prevent building large lists of data in memory
                        fileWriter.WriteLine(row);
                    }

                    fileWriter.Close();

                    // batch runs at 10k to avoid timeouts
                    // every 10k orders will be processed on a separate run
                    if (currentRunOrderCount >= 10000)
                    {
                        _serviceBusService.InitServiceBus();

                        periodInfo.StartAtDataID = lastId.ToString();
                        periodInfo.StartAtOriginID = shop.ShopID;

                        // send information to AF to continue this processing on another instance
                        await _serviceBusService.SendMessage(JsonConvert.SerializeObject(periodInfo));

                        await _serviceBusService.DisposeServiceBus();
                        return ProcessingOutcome.ProcessingToContinue;
                    }
                }
                while (orderCountPreFilter >= 250);

                lastId = 0;

                // populate refunds data
                var refundOrdersBatch = new List<Order>();
                orderCountPreFilter = 0;

                do
                {
                    var refundOrders = await _shopifyService.GetOrders(shop.ShopID,
                        shop.DataAccessToken,
                        periodInfo.StartDate.AddDays(-3).ToString("yyyy-MM-dd"),
                        periodInfo.EndDate.AddDays(3).ToString("yyyy-MM-dd"),
                        lastId, true);
                    if (refundOrders.Orders == null)
                    {
                        // invalidated/uninstalled company
                        if (refundOrders.Message != null && (refundOrders.Message.ToLower().Contains("invalid api key") || refundOrders.Message.ToLower().Contains("access token")))
                        {
                            _context.ShopifyMappings.RemoveRange(shop.ShopifyMappings.ToList());
                            _context.ShopifyShops.Remove(shop);
                            await _context.SaveChangesAsync();

                            await _errorLogger.LogError(new Exception(refundOrders.Message), OriginSource.TaxuallyAPI, periodInfo.RunTimestamp, $"Company mapping ({periodInfo.CompanyId}) {periodInfo.CompanyName} has been removed due to Shopify App being uninstalled.");
                            break;
                        }

                        periodHasData = false; // do not upload if errors
                        await _errorLogger.LogError(new Exception(refundOrders.Message), OriginSource.TaxuallyAPI, periodInfo.RunTimestamp, $"Failed to get Shopify refunds from API for company ({periodInfo.CompanyId}) {periodInfo.CompanyName} and period {periodInfo.StartDate} - {periodInfo.EndDate}.");
                        break;
                    }

                    orderCountPreFilter = refundOrders.Orders.Count;
                    refundOrdersBatch = refundOrders.Orders
                            .Where(x => x.CreatedAt.Date >= periodInfo.StartDate
                                && x.CreatedAt.Date <= periodInfo.EndDate)
                            .ToList();
                    if (refundOrdersBatch.Count > 0)
                        periodHasData = true;

                    totalOrderCount += refundOrdersBatch.Count;
                    lastId = refundOrders.Orders.Select(x => x.Id).LastOrDefault();

                    if (dbPeriod != null) // update DB with progress
                    {
                        dbPeriod.TotalOrdersPulled = totalOrderCount;
                        await _context.SaveChangesAsync();
                    }

                    // build refunds CSV
                    string csvDataHeader = "BusinessPartnerId,BusinessPartnerName,BusinessPartnerCountry,BusinessPartnerVatNumber,PostalCode,InvoiceId,DocumentNo,Description,Description2,ClientTaxCode," +
                        "SaleArrivalCountry,CurrencyCode,TransactionDate,GrossAmount,NetAmount,VatAmount,SaleDepartureCountry,SalesChannel,Quantity,VatRate,SKU,TransactionType,OriginalInvoiceId,DataSource," +
                        "Gateway,StoreRef";

                    // create file if needed
                    bool fileExists = File.Exists(tempCSVFilePath);
                    StreamWriter fileWriter = new StreamWriter(tempCSVFilePath, true);
                    if (!fileExists)
                        fileWriter.WriteLine(csvDataHeader);

                    var companyData = new List<CSVData>();

                    foreach (var order in refundOrdersBatch)
                    {
                        var refunds = order.Refunds;

                        foreach (var item in refunds.Where(x => x.CreatedAt >= periodInfo.StartDate && x.CreatedAt <= periodInfo.EndDate))
                        {
                            companyData.Add(new CSVData
                            {
                                InvoiceId = item.Id.ToString(),
                                BusinessPartnerCountry = order.BillingAddress?.CountryCode,
                                BusinessPartnerId = order.Customer?.Id.ToString(),
                                BusinessPartnerName = "",
                                CountryType = "",
                                PostalCode = order.ShippingAddress?.Zip,
                                SaleArrivalCountry = order.ShippingAddress?.CountryCode,
                                CurrencyCode = order.PresentmentCurrency,
                                TransactionDate = item.ProcessedAt,
                                DocumentNo = order.OrderNumber.ToString(),
                                Description = "",
                                ClientTaxCode = "",
                                GrossAmount = item.Transactions?.Sum(x => decimal.Parse(x.Amount)) ?? 0,
                                NetAmount = null,
                                VatAmount = 0,
                                Quantity = null,
                                VatRate = null,
                                SKU = "",
                                OriginalInvoiceId = item.OrderId.ToString(),
                                TransactionType = "Refund",
                                DataSource = "Shopify",
                                Gateway = order.Gateway
                            });
                        }
                    }


                    foreach (var data in companyData)
                    {
                        string row = $"\"{data.BusinessPartnerId}\",\"{data.BusinessPartnerName}\",\"{data.BusinessPartnerCountry}\",\"\",\"{data.PostalCode}\",\"{data.InvoiceId}\"," +
                            $"\"{data.DocumentNo}\",\"{data.Description}\",\"{data.Description2}\",\"{data.ClientTaxCode}\",\"{data.SaleArrivalCountry}\",\"{data.CurrencyCode}\",\"{data.TransactionDate.ToString("M/d/yyyy")}\"," +
                            $"\"{data.GrossAmount}\",\"{data.NetAmount}\",\"{data.VatAmount}\",\"\",\"\",\"{data.Quantity}\",\"{data.VatRate}\",\"{data.SKU}\"," +
                            $"\"{data.TransactionType}\",\"{data.OriginalInvoiceId}\",\"{data.DataSource}\",\"{data.Gateway}\",\"{shop.ShopID}\"";
                        
                        // write data on each loop to prevent building large lists of data in memory
                        fileWriter.WriteLine(row);
                    }

                    fileWriter.Close();
                }
                while (orderCountPreFilter >= 250);
            }

            return periodHasData ? ProcessingOutcome.ProcessedWithData : ProcessingOutcome.ProcessedWithoutData;
        }
    }
}
