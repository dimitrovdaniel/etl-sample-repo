using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodatExtractor.DAL.Models.Codat
{
    public class CreditNoteLineItem
    {
        [JsonProperty("description")]
        public string Description { get; set; }

        [JsonProperty("unitAmount")]
        public decimal UnitAmount { get; set; }

        [JsonProperty("quantity")]
        public decimal Quantity { get; set; }

        [JsonProperty("discountAmount")]
        public decimal DiscountAmount { get; set; }

        [JsonProperty("subTotal")]
        public decimal SubTotal { get; set; }

        [JsonProperty("taxAmount")]
        public decimal TaxAmount { get; set; }

        [JsonProperty("totalAmount")]
        public decimal TotalAmount { get; set; }

        [JsonProperty("accountRef")]
        public AccountRef AccountRef { get; set; }

        [JsonProperty("discountPercentage")]
        public decimal DiscountPercentage { get; set; }

        [JsonProperty("taxRateRef")]
        public TaxRateRef TaxRateRef { get; set; }

        [JsonProperty("itemRef")]
        public ItemRef ItemRef { get; set; }

        [JsonProperty("trackingCategoryRefs")]
        public List<TrackingCategoryRef> TrackingCategoryRefs { get; set; }

        [JsonProperty("tracking")]
        public Tracking Tracking { get; set; }

        [JsonProperty("isDirectIncome")]
        public bool IsDirectIncome { get; set; }
    }
    public class CompanyCreditNote
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("creditNoteNumber")]
        public string CreditNoteNumber { get; set; }

        [JsonProperty("customerRef")]
        public CustomerRef CustomerRef { get; set; }

        [JsonProperty("withholdingTax")]
        public List<WithholdingTax> WithholdingTax { get; set; }

        [JsonProperty("totalAmount")]
        public decimal TotalAmount { get; set; }

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

        [JsonProperty("discountPercentage")]
        public decimal DiscountPercentage { get; set; }

        [JsonProperty("remainingCredit")]
        public decimal RemainingCredit { get; set; }

        [JsonProperty("status")]
        public string Status { get; set; }

        [JsonProperty("issueDate")]
        public DateTime IssueDate { get; set; }

        [JsonProperty("allocatedOnDate")]
        public DateTime AllocatedOnDate { get; set; }

        [JsonProperty("currency")]
        public string Currency { get; set; }

        [JsonProperty("currencyRate")]
        public decimal CurrencyRate { get; set; }

        [JsonProperty("lineItems")]
        public List<CreditNoteLineItem> LineItems { get; set; }

        [JsonProperty("paymentAllocations")]
        public List<PaymentAllocation> PaymentAllocations { get; set; }

        [JsonProperty("modifiedDate")]
        public DateTime ModifiedDate { get; set; }

        [JsonProperty("sourceModifiedDate")]
        public DateTime SourceModifiedDate { get; set; }

        [JsonProperty("note")]
        public string Note { get; set; }

        [JsonProperty("metadata")]
        public Metadata Metadata { get; set; }

        [JsonProperty("supplementalData")]
        public SupplementalData SupplementalData { get; set; }
    }

    public class CompanyCreditNoteResponse : BaseCodatResponse
    {
        [JsonProperty("results")]
        public List<CompanyCreditNote> Results { get; set; }
    }
}
