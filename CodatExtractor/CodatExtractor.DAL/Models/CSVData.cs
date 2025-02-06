using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodatExtractor.DAL.Models
{
    public class CSVData
    {
        public string InvoiceId { get; set; }
        public string BusinessPartnerCountry {get;set; }
        public string BusinessPartnerId {get;set; }
        public string BusinessPartnerName {get;set; }
        public string CountryType {get;set; }
        public string PostalCode {get;set; }
        public string SaleArrivalCountry {get;set; }
        public string CurrencyCode {get;set; }
        public DateTime TransactionDate {get;set; }
        public string DocumentNo { get; internal set; }
        public string Description { get; internal set; }
        public string Description2 { get; internal set; }
        public string? ClientTaxCode { get; internal set; }
        public decimal GrossAmount { get; internal set; }
        public decimal? NetAmount { get; internal set; }
        public decimal VatAmount { get; internal set; }
        public decimal? Quantity { get; internal set; }
        public decimal? VatRate { get; internal set; }
        public string SKU { get; internal set; }
        public string BusinessPartnerVatNumber { get;  set; }
        public string TransactionType { get;  set; }
        public string DataSource { get;  set; }
        public string OriginalInvoiceId { get; set; }
        public string Gateway { get; set; }
    }
}
