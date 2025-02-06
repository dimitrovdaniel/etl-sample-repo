using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodatExtractor.DAL.Models.Codat
{
    public class AccountRef
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }
    }

    public class Allocation
    {
        [JsonProperty("currency")]
        public string Currency { get; set; }

        [JsonProperty("currencyRate")]
        public decimal CurrencyRate { get; set; }

        [JsonProperty("allocatedOnDate")]
        public DateTime AllocatedOnDate { get; set; }

        [JsonProperty("totalAmount")]
        public decimal TotalAmount { get; set; }
    }

    public class CategoryRef
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }
    }

    public class ItemRef
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }
    }

    public class LineItem
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

        [JsonProperty("discountPercentage")]
        public decimal DiscountPercentage { get; set; }

        [JsonProperty("accountRef")]
        public AccountRef AccountRef { get; set; }

        [JsonProperty("taxRateRef")]
        public TaxRateRef TaxRateRef { get; set; }

        [JsonProperty("itemRef")]
        public ItemRef ItemRef { get; set; }

        [JsonProperty("trackingCategoryRefs")]
        public List<TrackingCategoryRef> TrackingCategoryRefs { get; set; }

        [JsonProperty("tracking")]
        public Tracking Tracking { get; set; }

        [JsonProperty("isDirectCost")]
        public bool IsDirectCost { get; set; }
    }

    public class BillPayment
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("note")]
        public string Note { get; set; }

        [JsonProperty("reference")]
        public string Reference { get; set; }

        [JsonProperty("accountRef")]
        public AccountRef AccountRef { get; set; }

        [JsonProperty("currency")]
        public string Currency { get; set; }

        [JsonProperty("currencyRate")]
        public decimal CurrencyRate { get; set; }

        [JsonProperty("paidOnDate")]
        public DateTime PaidOnDate { get; set; }

        [JsonProperty("totalAmount")]
        public decimal TotalAmount { get; set; }
    }

    public class PaymentAllocation
    {
        [JsonProperty("payment")]
        public BillPayment Payment { get; set; }

        [JsonProperty("allocation")]
        public Allocation Allocation { get; set; }
    }


    public class ProjectRef
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }
    }

    public class PurchaseOrderRef
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("purchaseOrderNumber")]
        public string PurchaseOrderNumber { get; set; }
    }

    public class CompanyBill
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("reference")]
        public string Reference { get; set; }

        [JsonProperty("supplierRef")]
        public SupplierRef SupplierRef { get; set; }

        [JsonProperty("purchaseOrderRefs")]
        public List<PurchaseOrderRef> PurchaseOrderRefs { get; set; }

        [JsonProperty("issueDate")]
        public DateTime IssueDate { get; set; }

        [JsonProperty("dueDate")]
        public DateTime DueDate { get; set; }

        [JsonProperty("currency")]
        public string Currency { get; set; }

        [JsonProperty("currencyRate")]
        public decimal CurrencyRate { get; set; }

        [JsonProperty("lineItems")]
        public List<LineItem> LineItems { get; set; }

        [JsonProperty("withholdingTax")]
        public List<WithholdingTax> WithholdingTax { get; set; }

        [JsonProperty("status")]
        public string Status { get; set; }

        [JsonProperty("subTotal")]
        public decimal SubTotal { get; set; }

        [JsonProperty("taxAmount")]
        public decimal TaxAmount { get; set; }

        [JsonProperty("totalAmount")]
        public decimal TotalAmount { get; set; }

        [JsonProperty("amountDue")]
        public decimal AmountDue { get; set; }

        [JsonProperty("modifiedDate")]
        public DateTime ModifiedDate { get; set; }

        [JsonProperty("sourceModifiedDate")]
        public DateTime SourceModifiedDate { get; set; }

        [JsonProperty("note")]
        public string Note { get; set; }

        [JsonProperty("paymentAllocations")]
        public List<PaymentAllocation> PaymentAllocations { get; set; }

        [JsonProperty("metadata")]
        public Metadata Metadata { get; set; }

        [JsonProperty("supplementalData")]
        public SupplementalData SupplementalData { get; set; }
    }

    public class CompanyBillResponse : BaseCodatResponse
    {
        [JsonProperty("results")]
        public List<CompanyBill> Results { get; set; }
    }

    public class Tracking
    {
        [JsonProperty("categoryRefs")]
        public List<CategoryRef> CategoryRefs { get; set; }

        [JsonProperty("customerRef")]
        public CustomerRef CustomerRef { get; set; }

        [JsonProperty("projectRef")]
        public ProjectRef ProjectRef { get; set; }

        [JsonProperty("isBilledTo")]
        public string IsBilledTo { get; set; }

        [JsonProperty("isRebilledTo")]
        public string IsRebilledTo { get; set; }
    }

    public class TrackingCategoryRef
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }
    }

    public class WithholdingTax
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("amount")]
        public decimal Amount { get; set; }
    }
}
