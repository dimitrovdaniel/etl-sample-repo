using CodatExtractor.DAL.Entities;
using CodatExtractor.DAL.Entities.Models;
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
    public class CodatPeriodProcessor : IPeriodProcessor
    {
        private COEXTRContext _context;
        private CodatAPIService _codatService;
        private ServiceBusService _serviceBusService;
        private ErrorLoggingService _errorLogger;

        public OriginSource OriginSource { get; set; }

        public CodatPeriodProcessor(COEXTRContext context, CodatAPIService codatService, ServiceBusService serviceBusService, ErrorLoggingService errorLogger)
        {
            _context = context;
            _codatService = codatService;
            _serviceBusService = serviceBusService;
            _errorLogger = errorLogger;

            OriginSource = OriginSource.CodatAPI;
        }

        public async Task<ProcessingOutcome> ProcessPeriod(ProcessPeriodDTO periodInfo, string tempCSVFilePath)
        {
            if (periodInfo.CodatCompanyId == null) // not a Codat run, skip
                return ProcessingOutcome.ProcessedWithoutData;

            var dbPeriod = _context.RunCompanyPeriods.FirstOrDefault(x => x.RunTimestamp == periodInfo.RunTimestamp && x.PeriodID == periodInfo.PeriodId);

            // get run info and company connections
            var connections = await _codatService.GetCompanyConnections(periodInfo.CodatCompanyId);

            if (connections.Error || connections.Results == null || connections.Results.Count == 0)
            {
                // log error and stop
                await _errorLogger.LogError(null, OriginSource.CodatAPI, periodInfo.RunTimestamp, $"Failed to get Codat connections for company ({periodInfo.CompanyId}) {periodInfo.CompanyName}.");
                return ProcessingOutcome.ProcessingError;
            }

            bool periodHasData = false;

            // process connections
            foreach (var con in connections.Results)
            {
                // skip non-commerce integrations
                if (con.SourceType != "Commerce")
                    continue;

                // skip Codat extraction for Shopify companies, integration will not work
                if (con.PlatformName == "Shopify")
                    continue;

                List<CSVData> companyData = new List<CSVData>();

                // process orders
                var orders = await _codatService.GetOrdersForConnection(periodInfo.CodatCompanyId, con.Id, periodInfo.StartDate.ToString("yyyy-MM-dd"), periodInfo.EndDate.ToString("yyyy-MM-dd"));

                if (dbPeriod != null && orders.Results != null)
                {
                    dbPeriod.TotalOrdersPulled = orders.Results.Count;

                    if(orders.Results.Count > 0)
                    {
                        periodHasData = true;
                    }
                }

                // live update period data
                await _context.SaveChangesAsync();

                if (orders.Error || orders.Results == null)
                {
                    await _errorLogger.LogError(null, OriginSource.CodatAPI, periodInfo.RunTimestamp, $"Failed to get Codat orders for company ({periodInfo.CompanyId}) {periodInfo.CompanyName} and period {periodInfo.StartDate.ToString("yyyy-MM-dd")} - {periodInfo.EndDate.ToString("yyyy-MM-dd")}.");
                    continue;
                }

                // process orders
                foreach (var item in orders.Results)
                {
                    bool noCustomerForOrder = item.CustomerRef == null;
                    var customer = noCustomerForOrder ? null : await _codatService.GetCustomerForConnection(periodInfo.CodatCompanyId, con.Id, item.CustomerRef.Id);
                    noCustomerForOrder = customer == null || customer.Error;

                    if (item.OrderLineItems == null || item.OrderLineItems.Count == 0)
                    {
                        await _errorLogger.LogError(null, OriginSource.CodatAPI, periodInfo.RunTimestamp, $"No Codat order line items for company ({periodInfo.CompanyId}) {periodInfo.CompanyName} and order {item.OrderNumber} and period {periodInfo.StartDate.ToString("yyyy-MM-dd")} - {periodInfo.EndDate.ToString("yyyy-MM-dd")}.");
                        continue;
                    }

                    foreach (var orderItem in item.OrderLineItems)
                    {
                        companyData.Add(new CSVData
                        {
                            InvoiceId = item.OrderNumber,
                            BusinessPartnerCountry = noCustomerForOrder ? item.Country : customer?.Addresses?.Select(x => x.Country).FirstOrDefault(),
                            BusinessPartnerId = noCustomerForOrder ? "" : customer.Id,
                            BusinessPartnerName = noCustomerForOrder ? "" : customer?.CustomerName,
                            CountryType = noCustomerForOrder ? "" : customer?.Addresses?.Select(x => x.Type).FirstOrDefault(),
                            PostalCode = noCustomerForOrder ? "" : customer?.Addresses?.Select(x => x.PostalCode).FirstOrDefault(),
                            SaleArrivalCountry = item.Country,
                            CurrencyCode = item.Currency,
                            TransactionDate = !item.OrderLineItems.Any(x => x.Id.StartsWith("Refund_")) ? item.CreatedDate : item.SourceModifiedDate,
                            DocumentNo = orderItem.Id,
                            Description = orderItem.ProductRef?.Name,
                            ClientTaxCode = orderItem.Taxes?.Select(x => x.TaxComponentRef?.Name).FirstOrDefault(),
                            GrossAmount = orderItem.TotalAmount,
                            NetAmount = orderItem.TotalAmount - orderItem.TotalTaxAmount,
                            VatAmount = orderItem.TotalTaxAmount,
                            Quantity = orderItem.Quantity,
                            VatRate = orderItem.TaxPercentage,
                            SKU = orderItem.ProductVariantRef?.Id
                        });
                    }
                }


                // process bills
                var bills = await _codatService.GetBillsForCompany(periodInfo.CompanyName, periodInfo.StartDate.ToString("yyyy-MM-dd"), periodInfo.EndDate.ToString("yyyy-MM-dd"));

                if (dbPeriod != null && bills.Results != null)
                    dbPeriod.TotalBillsPulled = bills.Results.Count;

                // live update period data
                await _context.SaveChangesAsync();

                if (bills.Error || bills.Results == null)
                {
                    await _errorLogger.LogError(null, OriginSource.CodatAPI, periodInfo.RunTimestamp, $"Failed to get Codat bills for company ({periodInfo.CompanyId}) {periodInfo.CompanyName} and period {periodInfo.StartDate.ToString("yyyy-MM-dd")} - {periodInfo.EndDate.ToString("yyyy-MM-dd")}.");
                }
                else
                {
                    foreach (var item in bills.Results)
                    {
                        var supplier = await _codatService.GetSupplier(periodInfo.CodatCompanyId, item.SupplierRef.Id);
                        if (supplier.Error)
                        {
                            await _errorLogger.LogError(null, OriginSource.CodatAPI, periodInfo.RunTimestamp, $"Failed to get Codat supplier info for company ({periodInfo.CompanyId}) {periodInfo.CompanyName} and bill {item.Id} and period {periodInfo.StartDate.ToString("yyyy-MM-dd")} - {periodInfo.EndDate.ToString("yyyy-MM-dd")}.");
                            continue;
                        }

                        foreach (var orderItem in item.LineItems)
                        {
                            companyData.Add(new CSVData
                            {
                                InvoiceId = item.Id,
                                BusinessPartnerCountry = supplier?.Addresses?.Select(x => x.Country).FirstOrDefault(),
                                BusinessPartnerId = supplier.Id,
                                BusinessPartnerName = supplier.SupplierName,
                                BusinessPartnerVatNumber = supplier.TaxNumber,
                                CountryType = supplier?.Addresses?.Select(x => x.Type).FirstOrDefault(),
                                PostalCode = supplier?.Addresses?.Select(x => x.PostalCode).FirstOrDefault(),
                                SaleArrivalCountry = null,
                                CurrencyCode = item.Currency,
                                TransactionDate = item.IssueDate,
                                DocumentNo = item.Reference,
                                Description = orderItem.Description,
                                ClientTaxCode = orderItem.TaxRateRef?.Name,
                                GrossAmount = orderItem.TotalAmount,
                                NetAmount = orderItem.SubTotal,
                                VatAmount = orderItem.TaxAmount,
                                Quantity = orderItem.Quantity,
                                VatRate = orderItem.TaxRateRef?.EffectiveTaxRate ?? 0,
                                SKU = null
                            });
                        }
                    }
                }

                // process bill credit notes
                var billCreditNotes = await _codatService.GetBillsCreditNotesForCompany(periodInfo.CompanyName, periodInfo.StartDate.ToString("yyyy-MM-dd"), periodInfo.EndDate.ToString("yyyy-MM-dd"));

                if (dbPeriod != null && billCreditNotes.Results != null)
                    dbPeriod.TotalBillCreditNotesPulled = billCreditNotes.Results.Count;

                // live update period data
                await _context.SaveChangesAsync();

                if (billCreditNotes.Error || billCreditNotes.Results == null)
                {
                    await _errorLogger.LogError(null, OriginSource.CodatAPI, periodInfo.RunTimestamp, $"Failed to get Codat bills credit notes for company ({periodInfo.CompanyId}) {periodInfo.CompanyName} and period {periodInfo.StartDate.ToString("yyyy-MM-dd")} - {periodInfo.EndDate.ToString("yyyy-MM-dd")}.");
                }
                else
                {
                    foreach (var item in billCreditNotes.Results)
                    {
                        var supplier = await _codatService.GetSupplier(periodInfo.CodatCompanyId, item.SupplierRef.Id);
                        if (supplier.Error)
                        {
                            await _errorLogger.LogError(null, OriginSource.CodatAPI, periodInfo.RunTimestamp, $"Failed to get Codat supplier info for company ({periodInfo.CompanyId}) {periodInfo.CompanyName} and bill credit note {item.Id} and period {periodInfo.StartDate.ToString("yyyy-MM-dd")} - {periodInfo.EndDate.ToString("yyyy-MM-dd")}.");
                            continue;
                        }

                        foreach (var orderItem in item.LineItems)
                        {
                            companyData.Add(new CSVData
                            {
                                InvoiceId = item.Id,
                                BusinessPartnerCountry = supplier?.Addresses?.Select(x => x.Country).FirstOrDefault(),
                                BusinessPartnerId = supplier.Id,
                                BusinessPartnerName = supplier.SupplierName,
                                BusinessPartnerVatNumber = supplier.TaxNumber,
                                CountryType = supplier?.Addresses?.Select(x => x.Type).FirstOrDefault(),
                                PostalCode = supplier?.Addresses?.Select(x => x.PostalCode).FirstOrDefault(),
                                SaleArrivalCountry = null,
                                CurrencyCode = item.Currency,
                                TransactionDate = item.IssueDate,
                                DocumentNo = item.BillCreditNoteNumber,
                                Description = orderItem.Description,
                                Description2 = item.Status,
                                ClientTaxCode = orderItem.TaxRateRef?.Name,
                                GrossAmount = orderItem.TotalAmount,
                                NetAmount = orderItem.SubTotal,
                                VatAmount = orderItem.TaxAmount,
                                Quantity = orderItem.Quantity,
                                VatRate = orderItem.TaxRateRef?.EffectiveTaxRate ?? 0,
                                SKU = null
                            });
                        }
                    }
                }

                // process invoices
                var invoices = await _codatService.GetInvoicesForCompany(periodInfo.CompanyName, periodInfo.StartDate.ToString("yyyy-MM-dd"), periodInfo.EndDate.ToString("yyyy-MM-dd"));

                if (dbPeriod != null && invoices.Results != null)
                    dbPeriod.TotalInvoicesPulled = invoices.Results.Count;

                // live update period data
                await _context.SaveChangesAsync();

                if (invoices.Error || invoices.Results == null)
                {
                    await _errorLogger.LogError(null, OriginSource.CodatAPI, periodInfo.RunTimestamp, $"Failed to get Codat invoices for company ({periodInfo.CompanyId}) {periodInfo.CompanyName} and period {periodInfo.StartDate.ToString("yyyy-MM-dd")} - {periodInfo.EndDate.ToString("yyyy-MM-dd")}.");
                }
                else
                {
                    foreach (var item in invoices.Results)
                    {
                        var customer = await _codatService.GetCustomerForConnection(periodInfo.CodatCompanyId, con.Id, item.CustomerRef.Id);
                        if (customer.Error)
                        {
                            await _errorLogger.LogError(new Exception(customer.Message), OriginSource.CodatAPI, periodInfo.RunTimestamp, $"Failed to get Codat customer info for company ({periodInfo.CompanyId}) {periodInfo.CompanyName} and invoice {item.Id} and period {periodInfo.StartDate.ToString("yyyy-MM-dd")} - {periodInfo.EndDate.ToString("yyyy-MM-dd")}.");
                            continue;
                        }

                        foreach (var orderItem in item.LineItems)
                        {
                            companyData.Add(new CSVData
                            {
                                InvoiceId = item.Id,
                                BusinessPartnerCountry = customer?.Addresses?.Select(x => x.Country).FirstOrDefault(),
                                BusinessPartnerId = customer.Id,
                                BusinessPartnerName = customer?.CustomerName,
                                CountryType = customer?.Addresses?.Select(x => x.Type).FirstOrDefault(),
                                PostalCode = customer?.Addresses?.Select(x => x.PostalCode).FirstOrDefault(),
                                SaleArrivalCountry = null,
                                CurrencyCode = item.Currency,
                                TransactionDate = item.IssueDate,
                                DocumentNo = item.InvoiceNumber,
                                Description = orderItem.Description,
                                Description2 = item.Status,
                                ClientTaxCode = orderItem.TaxRateRef?.Name,
                                GrossAmount = orderItem.TotalAmount,
                                NetAmount = orderItem.SubTotal,
                                VatAmount = orderItem.TaxAmount,
                                Quantity = orderItem.Quantity,
                                VatRate = orderItem.TaxRateRef?.EffectiveTaxRate ?? 0,
                                SKU = null
                            });
                        }
                    }
                }

                // process credit notes
                var creditNotes = await _codatService.GetCreditNotesForCompany(periodInfo.CompanyName, periodInfo.StartDate.ToString("yyyy-MM-dd"), periodInfo.EndDate.ToString("yyyy-MM-dd"));

                if (dbPeriod != null && creditNotes.Results != null)
                    dbPeriod.TotalCreditNotesPulled = creditNotes.Results.Count;

                // live update period data
                await _context.SaveChangesAsync();

                if (creditNotes.Error || creditNotes.Results == null)
                {
                    await _errorLogger.LogError(null, OriginSource.CodatAPI, periodInfo.RunTimestamp, $"Failed to get Codat credit notes for company ({periodInfo.CompanyId}) {periodInfo.CompanyName} and period {periodInfo.StartDate.ToString("yyyy-MM-dd")} - {periodInfo.EndDate.ToString("yyyy-MM-dd")}.");
                }
                else
                {
                    foreach (var item in creditNotes.Results)
                    {
                        var customer = await _codatService.GetCustomerForConnection(periodInfo.CodatCompanyId, con.Id, item.CustomerRef.Id);
                        if (customer.Error)
                        {
                            await _errorLogger.LogError(null, OriginSource.CodatAPI, periodInfo.RunTimestamp, $"Failed to get Codat customer info for company ({periodInfo.CompanyId}) {periodInfo.CompanyName} and credit note {item.Id} and period {periodInfo.StartDate.ToString("yyyy-MM-dd")} - {periodInfo.EndDate.ToString("yyyy-MM-dd")}.");
                            continue;
                        }

                        foreach (var orderItem in item.LineItems)
                        {
                            companyData.Add(new CSVData
                            {
                                InvoiceId = item.Id,
                                BusinessPartnerCountry = customer.Addresses?.Select(x => x.Country).FirstOrDefault(),
                                BusinessPartnerId = customer.Id,
                                BusinessPartnerName = customer?.CustomerName,
                                CountryType = customer?.Addresses?.Select(x => x.Type).FirstOrDefault(),
                                PostalCode = customer?.Addresses?.Select(x => x.PostalCode).FirstOrDefault(),
                                SaleArrivalCountry = null,
                                CurrencyCode = item.Currency,
                                TransactionDate = item.IssueDate,
                                DocumentNo = item.CreditNoteNumber,
                                Description = orderItem.Description,
                                Description2 = item.Status,
                                ClientTaxCode = orderItem.TaxRateRef?.Name,
                                GrossAmount = orderItem.TotalAmount,
                                NetAmount = orderItem.SubTotal,
                                VatAmount = orderItem.TaxAmount,
                                Quantity = orderItem.Quantity,
                                VatRate = orderItem.TaxRateRef?.EffectiveTaxRate ?? 0,
                                SKU = null
                            });
                        }
                    }
                }

                // build CSV from Codat
                string csvDataHeader = "BusinessPartnerId,BusinessPartnerName,BusinessPartnerCountry,BusinessPartnerVatNumber,PostalCode,InvoiceId,DocumentNo,Description,Description2,ClientTaxCode," +
                    "SaleArrivalCountry,CurrencyCode,TransactionDate,GrossAmount,NetAmount,VatAmount,SaleDepartureCountry,SalesChannel,Quantity,VatRate,SKU";

                // create file if needed
                bool fileExists = File.Exists(tempCSVFilePath);
                StreamWriter targetFile = new StreamWriter(tempCSVFilePath, true);

                if (!fileExists)
                    targetFile.WriteLine(csvDataHeader);

                // write to file
                foreach (var data in companyData)
                {
                    string row = $"\"{data.BusinessPartnerId}\",\"{data.BusinessPartnerName}\",\"{data.BusinessPartnerCountry}\",\"\",\"{data.PostalCode}\",\"{data.InvoiceId}\"," +
                        $"\"{data.DocumentNo}\",\"{data.Description}\",\"{data.Description2}\",\"{data.ClientTaxCode}\",\"{data.SaleArrivalCountry}\",\"{data.CurrencyCode}\",\"{data.TransactionDate.ToString("M/d/yyyy")}\"," +
                        $"\"{data.GrossAmount}\",\"{data.NetAmount}\",\"{data.VatAmount}\",\"\",\"\",\"{data.Quantity}\",\"{data.VatRate}\",\"{data.SKU}\"";

                    // write each item to prevent memory issues if stored in a list
                    targetFile.WriteLine(row);
                }

                targetFile.Close();
            }

            return periodHasData ? ProcessingOutcome.ProcessedWithData : ProcessingOutcome.ProcessedWithoutData;
        }
    }
}
