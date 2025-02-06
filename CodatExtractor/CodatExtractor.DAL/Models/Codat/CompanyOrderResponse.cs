using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodatExtractor.DAL.Models.Codat
{
    public class CustomerRef
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }
    }

    public class DiscountAllocation
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("totalAmount")]
        public decimal TotalAmount { get; set; }
    }

    public class LocationRef
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }
    }

    public class OrderLineItem
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("quantity")]
        public decimal Quantity { get; set; }

        [JsonProperty("taxPercentage")]
        public decimal TaxPercentage { get; set; }

        [JsonProperty("totalAmount")]
        public decimal TotalAmount { get; set; }

        [JsonProperty("totalTaxAmount")]
        public decimal TotalTaxAmount { get; set; }

        [JsonProperty("unitPrice")]
        public decimal UnitPrice { get; set; }

        [JsonProperty("taxes")]
        public List<Taxis> Taxes { get; set; }

        [JsonProperty("productRef")]
        public ProductRef ProductRef { get; set; }

        [JsonProperty("productVariantRef")]
        public ProductVariantRef ProductVariantRef { get; set; }

        [JsonProperty("discountAllocations")]
        public List<DiscountAllocation> DiscountAllocations { get; set; }
    }

    public class Payment
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("amount")]
        public decimal Amount { get; set; }

        [JsonProperty("currency")]
        public string Currency { get; set; }

        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("status")]
        public string Status { get; set; }

        [JsonProperty("paymentProvider")]
        public string PaymentProvider { get; set; }

        [JsonProperty("dueDate")]
        public DateTime DueDate { get; set; }

        [JsonProperty("createdDate")]
        public DateTime CreatedDate { get; set; }

        [JsonProperty("modifiedDate")]
        public DateTime ModifiedDate { get; set; }

        [JsonProperty("sourceModifiedDate")]
        public DateTime SourceModifiedDate { get; set; }
    }

    public class ProductRef
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }
    }

    public class ProductVariantRef
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }
    }

    public class CompanyOrder
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("orderNumber")]
        public string OrderNumber { get; set; }

        [JsonProperty("country")]
        public string Country { get; set; }

        [JsonProperty("currency")]
        public string Currency { get; set; }

        [JsonProperty("createdDate")]
        public DateTime CreatedDate { get; set; }

        [JsonProperty("closedDate")]
        public DateTime ClosedDate { get; set; }

        [JsonProperty("totalAmount")]
        public decimal TotalAmount { get; set; }

        [JsonProperty("totalRefund")]
        public decimal TotalRefund { get; set; }

        [JsonProperty("totalTaxAmount")]
        public decimal TotalTaxAmount { get; set; }

        [JsonProperty("totalDiscount")]
        public decimal TotalDiscount { get; set; }

        [JsonProperty("totalGratuity")]
        public decimal TotalGratuity { get; set; }

        [JsonProperty("orderLineItems")]
        public List<OrderLineItem> OrderLineItems { get; set; }

        [JsonProperty("payments")]
        public List<Payment> Payments { get; set; }

        [JsonProperty("serviceCharges")]
        public List<ServiceCharge> ServiceCharges { get; set; }

        [JsonProperty("locationRef")]
        public LocationRef LocationRef { get; set; }

        [JsonProperty("customerRef")]
        public CustomerRef CustomerRef { get; set; }

        [JsonProperty("modifiedDate")]
        public DateTime ModifiedDate { get; set; }

        [JsonProperty("sourceModifiedDate")]
        public DateTime SourceModifiedDate { get; set; }
    }

    public class CompanyOrderResponse : BaseCodatResponse
    {
        [JsonProperty("results")]
        public List<CompanyOrder> Results { get; set; }
    }

    public class ServiceCharge
    {
        [JsonProperty("description")]
        public string Description { get; set; }

        [JsonProperty("totalAmount")]
        public decimal TotalAmount { get; set; }

        [JsonProperty("taxPercentage")]
        public decimal TaxPercentage { get; set; }

        [JsonProperty("taxAmount")]
        public decimal TaxAmount { get; set; }

        [JsonProperty("taxes")]
        public List<Taxis> Taxes { get; set; }

        [JsonProperty("quantity")]
        public decimal Quantity { get; set; }

        [JsonProperty("type")]
        public string Type { get; set; }
    }

    public class TaxComponentRef
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }
    }

    public class Taxis
    {
        [JsonProperty("taxComponentRef")]
        public TaxComponentRef TaxComponentRef { get; set; }

        [JsonProperty("taxAmount")]
        public decimal TaxAmount { get; set; }
    }
}
