using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodatExtractor.DAL.Models.Codat
{
    public class CompanyInvoice
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("invoiceNumber")]
        public string InvoiceNumber { get; set; }

        [JsonProperty("customerRef")]
        public CustomerRef CustomerRef { get; set; }

        [JsonProperty("salesOrderRefs")]
        public List<SalesOrderRef> SalesOrderRefs { get; set; }

        [JsonProperty("issueDate")]
        public DateTime IssueDate { get; set; }

        [JsonProperty("dueDate")]
        public DateTime DueDate { get; set; }

        [JsonProperty("modifiedDate")]
        public DateTime ModifiedDate { get; set; }

        [JsonProperty("sourceModifiedDate")]
        public DateTime SourceModifiedDate { get; set; }

        [JsonProperty("paidOnDate")]
        public DateTime PaidOnDate { get; set; }

        [JsonProperty("currency")]
        public string Currency { get; set; }

        [JsonProperty("currencyRate")]
        public decimal CurrencyRate { get; set; }

        [JsonProperty("lineItems")]
        public List<LineItem> LineItems { get; set; }

        [JsonProperty("paymentAllocations")]
        public List<PaymentAllocation> PaymentAllocations { get; set; }

        [JsonProperty("withholdingTax")]
        public List<WithholdingTax> WithholdingTax { get; set; }

        [JsonProperty("totalDiscount")]
        public decimal TotalDiscount { get; set; }

        [JsonProperty("subTotal")]
        public decimal SubTotal { get; set; }

        [JsonProperty("additionalTaxAmount")]
        public decimal AdditionalTaxAmount { get; set; }

        [JsonProperty("additionalTaxPercentage")]
        public decimal AdditionalTaxPercentage { get; set; }

        [JsonProperty("totalTaxAmount")]
        public decimal TotalTaxAmount { get; set; }

        [JsonProperty("totalAmount")]
        public decimal TotalAmount { get; set; }

        [JsonProperty("amountDue")]
        public decimal AmountDue { get; set; }

        [JsonProperty("discountPercentage")]
        public decimal DiscountPercentage { get; set; }

        [JsonProperty("status")]
        public string Status { get; set; }

        [JsonProperty("note")]
        public string Note { get; set; }

        [JsonProperty("metadata")]
        public Metadata Metadata { get; set; }

        [JsonProperty("supplementalData")]
        public SupplementalData SupplementalData { get; set; }
    }

    public class CompanyInvoiceResponse : BaseCodatResponse
    {
        [JsonProperty("results")]
        public List<CompanyInvoice> Results { get; set; }
    }

    public class SalesOrderRef
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("dataType")]
        public string DataType { get; set; }
    }

}
