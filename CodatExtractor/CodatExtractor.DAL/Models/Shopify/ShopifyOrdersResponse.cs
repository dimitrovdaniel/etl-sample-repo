using Newtonsoft.Json;

namespace CodatExtractor.DAL.Models.Shopify
{
    // Root myDeserializedClass = JsonConvert.DeserializeObject<Root>(myJsonResponse);
    public class BillingAddress
    {
        [JsonProperty("province")]
        public string Province { get; set; }

        [JsonProperty("country")]
        public string Country { get; set; }

        [JsonProperty("latitude")]
        public double? Latitude { get; set; }

        [JsonProperty("longitude")]
        public double? Longitude { get; set; }

        [JsonProperty("country_code")]
        public string CountryCode { get; set; }

        [JsonProperty("province_code")]
        public string ProvinceCode { get; set; }
    }

    public class ClientDetails
    {
        [JsonProperty("accept_language")]
        public object AcceptLanguage { get; set; }

        [JsonProperty("browser_height")]
        public object BrowserHeight { get; set; }

        [JsonProperty("browser_ip")]
        public object BrowserIp { get; set; }

        [JsonProperty("browser_width")]
        public object BrowserWidth { get; set; }

        [JsonProperty("session_hash")]
        public object SessionHash { get; set; }

        [JsonProperty("user_agent")]
        public object UserAgent { get; set; }
    }

    public class CurrentSubtotalPriceSet
    {
        [JsonProperty("shop_money")]
        public ShopMoney ShopMoney { get; set; }

        [JsonProperty("presentment_money")]
        public PresentmentMoney PresentmentMoney { get; set; }
    }

    public class CurrentTotalDiscountsSet
    {
        [JsonProperty("shop_money")]
        public ShopMoney ShopMoney { get; set; }

        [JsonProperty("presentment_money")]
        public PresentmentMoney PresentmentMoney { get; set; }
    }

    public class CurrentTotalPriceSet
    {
        [JsonProperty("shop_money")]
        public ShopMoney ShopMoney { get; set; }

        [JsonProperty("presentment_money")]
        public PresentmentMoney PresentmentMoney { get; set; }
    }

    public class CurrentTotalTaxSet
    {
        [JsonProperty("shop_money")]
        public ShopMoney ShopMoney { get; set; }

        [JsonProperty("presentment_money")]
        public PresentmentMoney PresentmentMoney { get; set; }
    }

    public class Customer
    {
        [JsonProperty("id")]
        public long Id { get; set; }

        [JsonProperty("accepts_marketing")]
        public bool AcceptsMarketing { get; set; }

        [JsonProperty("created_at")]
        public DateTime CreatedAt { get; set; }

        [JsonProperty("updated_at")]
        public DateTime UpdatedAt { get; set; }

        [JsonProperty("state")]
        public string State { get; set; }

        [JsonProperty("note")]
        public string Note { get; set; }

        [JsonProperty("verified_email")]
        public bool VerifiedEmail { get; set; }

        [JsonProperty("multipass_identifier")]
        public object MultipassIdentifier { get; set; }

        [JsonProperty("tax_exempt")]
        public bool TaxExempt { get; set; }

        [JsonProperty("tags")]
        public string Tags { get; set; }

        [JsonProperty("currency")]
        public string Currency { get; set; }

        [JsonProperty("accepts_marketing_updated_at")]
        public DateTime AcceptsMarketingUpdatedAt { get; set; }

        [JsonProperty("marketing_opt_in_level")]
        public object MarketingOptInLevel { get; set; }

        [JsonProperty("tax_exemptions")]
        public List<object> TaxExemptions { get; set; }

        [JsonProperty("email_marketing_consent")]
        public EmailMarketingConsent EmailMarketingConsent { get; set; }

        [JsonProperty("sms_marketing_consent")]
        public object SmsMarketingConsent { get; set; }

        [JsonProperty("admin_graphql_api_id")]
        public string AdminGraphqlApiId { get; set; }
        [JsonProperty("first_name")]
        public string FirstName { get; set; }
        [JsonProperty("last_name")]
        public string LastName { get; set; }

        [JsonProperty("default_address")]
        public DefaultAddress DefaultAddress { get; set; }
    }

    public class DefaultAddress
    {
        [JsonProperty("id")]
        public long Id { get; set; }

        [JsonProperty("customer_id")]
        public long CustomerId { get; set; }

        [JsonProperty("company")]
        public string Company { get; set; }

        [JsonProperty("province")]
        public string Province { get; set; }

        [JsonProperty("country")]
        public string Country { get; set; }

        [JsonProperty("province_code")]
        public string ProvinceCode { get; set; }

        [JsonProperty("country_code")]
        public string CountryCode { get; set; }

        [JsonProperty("country_name")]
        public string CountryName { get; set; }

        [JsonProperty("default")]
        public bool Default { get; set; }
    }

    public class EmailMarketingConsent
    {
        [JsonProperty("state")]
        public string State { get; set; }

        [JsonProperty("opt_in_level")]
        public string OptInLevel { get; set; }

        [JsonProperty("consent_updated_at")]
        public object ConsentUpdatedAt { get; set; }
    }

    public class Fulfillment
    {
        [JsonProperty("id")]
        public object Id { get; set; }

        [JsonProperty("admin_graphql_api_id")]
        public string AdminGraphqlApiId { get; set; }

        [JsonProperty("created_at")]
        public DateTime CreatedAt { get; set; }

        [JsonProperty("location_id")]
        public long? LocationId { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("order_id")]
        public object OrderId { get; set; }

        [JsonProperty("origin_address")]
        public OriginAddress OriginAddress { get; set; }

        [JsonProperty("receipt")]
        public Receipt Receipt { get; set; }

        [JsonProperty("service")]
        public string Service { get; set; }

        [JsonProperty("shipment_status")]
        public object ShipmentStatus { get; set; }

        [JsonProperty("status")]
        public string Status { get; set; }

        [JsonProperty("tracking_company")]
        public object TrackingCompany { get; set; }

        [JsonProperty("tracking_number")]
        public object TrackingNumber { get; set; }

        [JsonProperty("tracking_numbers")]
        public List<object> TrackingNumbers { get; set; }

        [JsonProperty("tracking_url")]
        public object TrackingUrl { get; set; }

        [JsonProperty("tracking_urls")]
        public List<object> TrackingUrls { get; set; }

        [JsonProperty("updated_at")]
        public DateTime UpdatedAt { get; set; }

        [JsonProperty("line_items")]
        public List<LineItem> LineItems { get; set; }
    }

    public class LineItem
    {
        [JsonProperty("id")]
        public object Id { get; set; }

        [JsonProperty("admin_graphql_api_id")]
        public string AdminGraphqlApiId { get; set; }

        [JsonProperty("fulfillable_quantity")]
        public int FulfillableQuantity { get; set; }

        [JsonProperty("fulfillment_service")]
        public string FulfillmentService { get; set; }

        [JsonProperty("fulfillment_status")]
        public string FulfillmentStatus { get; set; }

        [JsonProperty("gift_card")]
        public bool GiftCard { get; set; }

        [JsonProperty("grams")]
        public int Grams { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("price")]
        public string Price { get; set; }

        [JsonProperty("price_set")]
        public PriceSet PriceSet { get; set; }

        [JsonProperty("product_exists")]
        public bool ProductExists { get; set; }

        [JsonProperty("product_id")]
        public object ProductId { get; set; }

        [JsonProperty("properties")]
        public List<object> Properties { get; set; }

        [JsonProperty("quantity")]
        public int Quantity { get; set; }

        [JsonProperty("requires_shipping")]
        public bool RequiresShipping { get; set; }

        [JsonProperty("sku")]
        public string Sku { get; set; }

        [JsonProperty("taxable")]
        public bool Taxable { get; set; }

        [JsonProperty("title")]
        public string Title { get; set; }

        [JsonProperty("total_discount")]
        public string TotalDiscount { get; set; }

        [JsonProperty("total_discount_set")]
        public TotalDiscountSet TotalDiscountSet { get; set; }

        [JsonProperty("variant_id")]
        public object VariantId { get; set; }

        [JsonProperty("variant_inventory_management")]
        public string VariantInventoryManagement { get; set; }

        [JsonProperty("variant_title")]
        public string VariantTitle { get; set; }

        [JsonProperty("vendor")]
        public string Vendor { get; set; }

        [JsonProperty("tax_lines")]
        public List<TaxLine> TaxLines { get; set; }

        [JsonProperty("duties")]
        public List<object> Duties { get; set; }

        [JsonProperty("discount_allocations")]
        public List<object> DiscountAllocations { get; set; }
    }

    public class Order
    {
        [JsonProperty("id")]
        public long Id { get; set; }

        [JsonProperty("admin_graphql_api_id")]
        public string AdminGraphqlApiId { get; set; }

        [JsonProperty("app_id")]
        public int AppId { get; set; }

        [JsonProperty("browser_ip")]
        public object BrowserIp { get; set; }

        [JsonProperty("buyer_accepts_marketing")]
        public bool BuyerAcceptsMarketing { get; set; }

        [JsonProperty("cancel_reason")]
        public object CancelReason { get; set; }

        [JsonProperty("cancelled_at")]
        public object CancelledAt { get; set; }

        [JsonProperty("cart_token")]
        public object CartToken { get; set; }

        [JsonProperty("checkout_id")]
        public object CheckoutId { get; set; }

        [JsonProperty("checkout_token")]
        public string CheckoutToken { get; set; }

        [JsonProperty("client_details")]
        public ClientDetails ClientDetails { get; set; }

        [JsonProperty("closed_at")]
        public DateTime? ClosedAt { get; set; }

        [JsonProperty("confirmed")]
        public bool Confirmed { get; set; }

        [JsonProperty("created_at")]
        public DateTime CreatedAt { get; set; }

        [JsonProperty("currency")]
        public string Currency { get; set; }

        [JsonProperty("current_subtotal_price")]
        public string CurrentSubtotalPrice { get; set; }

        [JsonProperty("current_subtotal_price_set")]
        public CurrentSubtotalPriceSet CurrentSubtotalPriceSet { get; set; }

        [JsonProperty("current_total_discounts")]
        public string CurrentTotalDiscounts { get; set; }

        [JsonProperty("current_total_discounts_set")]
        public CurrentTotalDiscountsSet CurrentTotalDiscountsSet { get; set; }

        [JsonProperty("current_total_duties_set")]
        public object CurrentTotalDutiesSet { get; set; }

        [JsonProperty("current_total_price")]
        public string CurrentTotalPrice { get; set; }

        [JsonProperty("current_total_price_set")]
        public CurrentTotalPriceSet CurrentTotalPriceSet { get; set; }

        [JsonProperty("current_total_tax")]
        public string CurrentTotalTax { get; set; }

        [JsonProperty("current_total_tax_set")]
        public CurrentTotalTaxSet CurrentTotalTaxSet { get; set; }

        [JsonProperty("customer_locale")]
        public string CustomerLocale { get; set; }

        [JsonProperty("device_id")]
        public object DeviceId { get; set; }

        [JsonProperty("discount_codes")]
        public List<object> DiscountCodes { get; set; }

        [JsonProperty("estimated_taxes")]
        public bool EstimatedTaxes { get; set; }

        [JsonProperty("financial_status")]
        public string FinancialStatus { get; set; }

        [JsonProperty("fulfillment_status")]
        public string FulfillmentStatus { get; set; }

        [JsonProperty("gateway")]
        public string Gateway { get; set; }

        [JsonProperty("landing_site")]
        public object LandingSite { get; set; }

        [JsonProperty("landing_site_ref")]
        public object LandingSiteRef { get; set; }

        [JsonProperty("location_id")]
        public long? LocationId { get; set; }

        [JsonProperty("merchant_of_record_app_id")]
        public object MerchantOfRecordAppId { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("note")]
        public object Note { get; set; }

        [JsonProperty("note_attributes")]
        public List<object> NoteAttributes { get; set; }

        [JsonProperty("number")]
        public int Number { get; set; }

        [JsonProperty("order_number")]
        public int OrderNumber { get; set; }

        [JsonProperty("order_status_url")]
        public string OrderStatusUrl { get; set; }

        [JsonProperty("original_total_duties_set")]
        public object OriginalTotalDutiesSet { get; set; }

        [JsonProperty("payment_gateway_names")]
        public List<string> PaymentGatewayNames { get; set; }

        [JsonProperty("presentment_currency")]
        public string PresentmentCurrency { get; set; }

        [JsonProperty("processed_at")]
        public DateTime ProcessedAt { get; set; }

        [JsonProperty("processing_method")]
        public string ProcessingMethod { get; set; }

        [JsonProperty("reference")]
        public object Reference { get; set; }

        [JsonProperty("referring_site")]
        public object ReferringSite { get; set; }

        [JsonProperty("source_identifier")]
        public object SourceIdentifier { get; set; }

        [JsonProperty("source_name")]
        public string SourceName { get; set; }

        [JsonProperty("source_url")]
        public object SourceUrl { get; set; }

        [JsonProperty("subtotal_price")]
        public string SubtotalPrice { get; set; }

        [JsonProperty("subtotal_price_set")]
        public SubtotalPriceSet SubtotalPriceSet { get; set; }

        [JsonProperty("tags")]
        public string Tags { get; set; }

        [JsonProperty("tax_lines")]
        public List<TaxLine> TaxLines { get; set; }

        [JsonProperty("taxes_included")]
        public bool TaxesIncluded { get; set; }

        [JsonProperty("test")]
        public bool Test { get; set; }

        [JsonProperty("token")]
        public string Token { get; set; }

        [JsonProperty("total_discounts")]
        public string TotalDiscounts { get; set; }

        [JsonProperty("total_discounts_set")]
        public TotalDiscountsSet TotalDiscountsSet { get; set; }

        [JsonProperty("total_line_items_price")]
        public string TotalLineItemsPrice { get; set; }

        [JsonProperty("total_line_items_price_set")]
        public TotalLineItemsPriceSet TotalLineItemsPriceSet { get; set; }

        [JsonProperty("total_outstanding")]
        public string TotalOutstanding { get; set; }

        [JsonProperty("total_price")]
        public string TotalPrice { get; set; }

        [JsonProperty("total_price_set")]
        public TotalPriceSet TotalPriceSet { get; set; }

        [JsonProperty("total_shipping_price_set")]
        public TotalShippingPriceSet TotalShippingPriceSet { get; set; }

        [JsonProperty("total_tax")]
        public string TotalTax { get; set; }

        [JsonProperty("total_tax_set")]
        public TotalTaxSet TotalTaxSet { get; set; }

        [JsonProperty("total_tip_received")]
        public string TotalTipReceived { get; set; }

        [JsonProperty("total_weight")]
        public int TotalWeight { get; set; }

        [JsonProperty("updated_at")]
        public DateTime UpdatedAt { get; set; }

        [JsonProperty("user_id")]
        public long? UserId { get; set; }

        [JsonProperty("billing_address")]
        public BillingAddress BillingAddress { get; set; }

        [JsonProperty("customer")]
        public Customer Customer { get; set; }

        [JsonProperty("discount_applications")]
        public List<object> DiscountApplications { get; set; }

        [JsonProperty("fulfillments")]
        public List<Fulfillment> Fulfillments { get; set; }

        [JsonProperty("line_items")]
        public List<LineItem> LineItems { get; set; }

        [JsonProperty("payment_terms")]
        public object PaymentTerms { get; set; }

        [JsonProperty("refunds")]
        public List<RefundData> Refunds { get; set; }

        [JsonProperty("shipping_address")]
        public ShippingAddress ShippingAddress { get; set; }

        [JsonProperty("shipping_lines")]
        public List<object> ShippingLines { get; set; }
    }

    public class OriginAddress
    {
    }

    public class PresentmentMoney
    {
        [JsonProperty("amount")]
        public string Amount { get; set; }

        [JsonProperty("currency_code")]
        public string CurrencyCode { get; set; }
    }

    public class PriceSet
    {
        [JsonProperty("shop_money")]
        public ShopMoney ShopMoney { get; set; }

        [JsonProperty("presentment_money")]
        public PresentmentMoney PresentmentMoney { get; set; }
    }

    public class Receipt
    {
    }

    public class ShopifyOrdersResponse : StatusMessage
    {
        [JsonProperty("orders")]
        public List<Order> Orders { get; set; }
    }

    public class ShippingAddress
    {
        [JsonProperty("province")]
        public string Province { get; set; }

        [JsonProperty("country")]
        public string Country { get; set; }

        [JsonProperty("company")]
        public string Company { get; set; }

        [JsonProperty("country_code")]
        public string CountryCode { get; set; }

        [JsonProperty("province_code")]
        public string ProvinceCode { get; set; }

        [JsonProperty("zip")]
        public string Zip { get; set; }
    }

    public class ShopMoney
    {
        [JsonProperty("amount")]
        public string Amount { get; set; }

        [JsonProperty("currency_code")]
        public string CurrencyCode { get; set; }
    }

    public class SubtotalPriceSet
    {
        [JsonProperty("shop_money")]
        public ShopMoney ShopMoney { get; set; }

        [JsonProperty("presentment_money")]
        public PresentmentMoney PresentmentMoney { get; set; }
    }

    public class TaxLine
    {
        [JsonProperty("price")]
        public string Price { get; set; }

        [JsonProperty("rate")]
        public double Rate { get; set; }

        [JsonProperty("title")]
        public string Title { get; set; }

        [JsonProperty("price_set")]
        public PriceSet PriceSet { get; set; }

        [JsonProperty("channel_liable")]
        public bool? ChannelLiable { get; set; }
    }

    public class TotalDiscountSet
    {
        [JsonProperty("shop_money")]
        public ShopMoney ShopMoney { get; set; }

        [JsonProperty("presentment_money")]
        public PresentmentMoney PresentmentMoney { get; set; }
    }

    public class TotalDiscountsSet
    {
        [JsonProperty("shop_money")]
        public ShopMoney ShopMoney { get; set; }

        [JsonProperty("presentment_money")]
        public PresentmentMoney PresentmentMoney { get; set; }
    }

    public class TotalLineItemsPriceSet
    {
        [JsonProperty("shop_money")]
        public ShopMoney ShopMoney { get; set; }

        [JsonProperty("presentment_money")]
        public PresentmentMoney PresentmentMoney { get; set; }
    }

    public class TotalPriceSet
    {
        [JsonProperty("shop_money")]
        public ShopMoney ShopMoney { get; set; }

        [JsonProperty("presentment_money")]
        public PresentmentMoney PresentmentMoney { get; set; }
    }

    public class TotalShippingPriceSet
    {
        [JsonProperty("shop_money")]
        public ShopMoney ShopMoney { get; set; }

        [JsonProperty("presentment_money")]
        public PresentmentMoney PresentmentMoney { get; set; }
    }

    public class TotalTaxSet
    {
        [JsonProperty("shop_money")]
        public ShopMoney ShopMoney { get; set; }

        [JsonProperty("presentment_money")]
        public PresentmentMoney PresentmentMoney { get; set; }
    }

    public class OriginLocation
    {
        [JsonProperty("id")]
        public long Id { get; set; }

        [JsonProperty("country_code")]
        public string CountryCode { get; set; }

        [JsonProperty("province_code")]
        public string ProvinceCode { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("address1")]
        public string Address1 { get; set; }

        [JsonProperty("address2")]
        public string Address2 { get; set; }

        [JsonProperty("city")]
        public string City { get; set; }

        [JsonProperty("zip")]
        public string Zip { get; set; }
    }

    public class RefundLineItem
    {
        [JsonProperty("id")]
        public long Id { get; set; }

        [JsonProperty("line_item_id")]
        public long LineItemId { get; set; }

        [JsonProperty("location_id")]
        public long? LocationId { get; set; }

        [JsonProperty("quantity")]
        public int Quantity { get; set; }

        [JsonProperty("restock_type")]
        public string RestockType { get; set; }

        [JsonProperty("subtotal")]
        public double Subtotal { get; set; }

        [JsonProperty("subtotal_set")]
        public SubtotalSet SubtotalSet { get; set; }

        [JsonProperty("total_tax")]
        public double TotalTax { get; set; }

        [JsonProperty("total_tax_set")]
        public TotalTaxSet TotalTaxSet { get; set; }

        [JsonProperty("line_item")]
        public LineItem LineItem { get; set; }
    }

    public class RefundData
    {
        [JsonProperty("id")]
        public long Id { get; set; }

        [JsonProperty("admin_graphql_api_id")]
        public string AdminGraphqlApiId { get; set; }

        [JsonProperty("created_at")]
        public DateTime CreatedAt { get; set; }

        [JsonProperty("note")]
        public object Note { get; set; }

        [JsonProperty("order_id")]
        public long OrderId { get; set; }

        [JsonProperty("processed_at")]
        public DateTime ProcessedAt { get; set; }

        [JsonProperty("restock")]
        public bool Restock { get; set; }

        [JsonProperty("total_duties_set")]
        public TotalDutiesSet TotalDutiesSet { get; set; }

        [JsonProperty("user_id")]
        public long? UserId { get; set; }

        [JsonProperty("order_adjustments")]
        public List<object> OrderAdjustments { get; set; }

        [JsonProperty("transactions")]
        public List<Transaction> Transactions { get; set; }

        [JsonProperty("refund_line_items")]
        public List<RefundLineItem> RefundLineItems { get; set; }

        [JsonProperty("duties")]
        public List<object> Duties { get; set; }
    }

    public class SubtotalSet
    {
        [JsonProperty("shop_money")]
        public ShopMoney ShopMoney { get; set; }

        [JsonProperty("presentment_money")]
        public PresentmentMoney PresentmentMoney { get; set; }
    }

    public class TotalDutiesSet
    {
        [JsonProperty("shop_money")]
        public ShopMoney ShopMoney { get; set; }

        [JsonProperty("presentment_money")]
        public PresentmentMoney PresentmentMoney { get; set; }
    }

    public class Transaction
    {
        [JsonProperty("id")]
        public long Id { get; set; }

        [JsonProperty("admin_graphql_api_id")]
        public string AdminGraphqlApiId { get; set; }

        [JsonProperty("amount")]
        public string Amount { get; set; }

        [JsonProperty("authorization")]
        public object Authorization { get; set; }

        [JsonProperty("created_at")]
        public DateTime CreatedAt { get; set; }

        [JsonProperty("currency")]
        public string Currency { get; set; }

        [JsonProperty("device_id")]
        public object DeviceId { get; set; }

        [JsonProperty("error_code")]
        public object ErrorCode { get; set; }

        [JsonProperty("gateway")]
        public string Gateway { get; set; }

        [JsonProperty("kind")]
        public string Kind { get; set; }

        [JsonProperty("location_id")]
        public long? LocationId { get; set; }

        [JsonProperty("message")]
        public string Message { get; set; }

        [JsonProperty("order_id")]
        public long OrderId { get; set; }

        [JsonProperty("parent_id")]
        public long ParentId { get; set; }

        [JsonProperty("processed_at")]
        public DateTime ProcessedAt { get; set; }

        [JsonProperty("receipt")]
        public Receipt Receipt { get; set; }

        [JsonProperty("source_name")]
        public string SourceName { get; set; }

        [JsonProperty("status")]
        public string Status { get; set; }

        [JsonProperty("test")]
        public bool Test { get; set; }

        [JsonProperty("user_id")]
        public long? UserId { get; set; }
    }


    public class CountResponse : StatusMessage
    {
        [JsonProperty("count")]
        public int Count { get; set; }
    }

}
