using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodatExtractor.DAL.Models.Codat
{
    public class CompanyBillCreditNote
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("billCreditNoteNumber")]
        public string BillCreditNoteNumber { get; set; }

        [JsonProperty("supplierRef")]
        public SupplierRef SupplierRef { get; set; }

        [JsonProperty("withholdingTax")]
        public List<WithholdingTax> WithholdingTax { get; set; }

        [JsonProperty("totalAmount")]
        public decimal TotalAmount { get; set; }

        [JsonProperty("totalDiscount")]
        public decimal TotalDiscount { get; set; }

        [JsonProperty("subTotal")]
        public decimal SubTotal { get; set; }

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
        public List<LineItem> LineItems { get; set; }

        [JsonProperty("paymentAllocations")]
        public List<PaymentAllocation> PaymentAllocations { get; set; }

        [JsonProperty("modifiedDate")]
        public DateTime ModifiedDate { get; set; }

        [JsonProperty("sourceModifiedDate")]
        public DateTime SourceModifiedDate { get; set; }

        [JsonProperty("metadata")]
        public Metadata Metadata { get; set; }

        [JsonProperty("supplementalData")]
        public SupplementalData SupplementalData { get; set; }

        [JsonProperty("note")]
        public string Note { get; set; }
    }

    public class CompanyBillCreditNoteResponse : BaseCodatResponse
    {
        [JsonProperty("results")]
        public List<CompanyBillCreditNote> Results { get; set; }
    }
}
