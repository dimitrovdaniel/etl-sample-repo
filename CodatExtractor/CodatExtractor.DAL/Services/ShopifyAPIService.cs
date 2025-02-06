using CodatExtractor.DAL.Entities;
using CodatExtractor.DAL.Entities.Models;
using CodatExtractor.DAL.Models;
using CodatExtractor.DAL.Models.Shopify;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodatExtractor.DAL.Services
{
    // gets and processes data from the Shopify API
    public class ShopifyAPIService
    {
        private COEXTRContext _context;
        private HttpClient _httpClient;
        private ErrorLoggingService _errorLogger;

        public ShopifyAPIService(COEXTRContext context, ErrorLoggingService errorLogger)
        {
            _context = context;
            _errorLogger = errorLogger;
            _httpClient = new HttpClient();
        }

        // saves token acquired from a user installation
        public async Task SaveShopToken(string shop, string token)
        {
            try
            {
                var existingShop = _context.ShopifyShops.FirstOrDefault(x => x.ShopID == shop);
                // create the shop if does not exist
                if(existingShop == null)
                {
                    existingShop = new ShopifyShopEntity
                    {
                        DateCreated = DateTime.UtcNow,
                        ShopID = shop
                    };

                    _context.ShopifyShops.Add(existingShop);
                }

                // update token
                existingShop.DataAccessToken = token;
                _context.SaveChanges();
            }
            catch (Exception ex)
            {
                // log error
                await _errorLogger.LogError(ex, OriginSource.ShopifyAPI, DateTime.UtcNow.ToString("yyyy-MM-dd-HH-mm-ss"));
            }
        }

        // get orders from Shopify
        public async Task<ShopifyOrdersResponse> GetOrders(string shopId, string shopToken, string monthStart, string monthEnd, long lastId = 0, bool refundsMode = false)
        {
            string responseError = null;

            try
            {
                string filterQuery = $"&created_at_min={monthStart}&created_at_max={monthEnd}&since_id={lastId}";

                // for refunds, modify date dates to get additional orders to check
                if(refundsMode)
                {
                    var createdMin = DateTime.ParseExact(monthEnd, "yyyy-MM-dd", null).AddMonths(-1);
                    filterQuery = $"&updated_at_min={monthStart}&updated_at_max={monthEnd}&since_id={lastId}&financial_status=refunded&created_at_max={monthStart}&created_at_min={createdMin.ToString("yyyy-MM-dd")}";
                }

                string url = $"https://{shopId}/admin/api/2022-10/orders.json?status=any&limit=250" + filterQuery;

                // specify shop by setting access token
                _httpClient.DefaultRequestHeaders.Remove("X-Shopify-Access-Token");
                _httpClient.DefaultRequestHeaders.Add("X-Shopify-Access-Token", shopToken);

                var response = await _httpClient.GetAsync(url);
                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var result = JsonConvert.DeserializeObject<ShopifyOrdersResponse>(content);
                    return result;
                }
                else
                {
                    // failed getting orders
                    var content = await response.Content.ReadAsStringAsync();
                    responseError = content;
                }
            }
            catch (Exception ex)
            {
                // log error
                await _errorLogger.LogError(ex, OriginSource.ShopifyAPI, DateTime.UtcNow.ToString("yyyy-MM-dd-HH-mm-ss"));
            }

            // couldn't get orders
            return new ShopifyOrdersResponse
            {
                Error = true,
                Message = responseError ?? "Couldn't get Shopify orders."
            };
        }
        
        // get order count
        public async Task<CountResponse> GetOrderCount(ShopifyShopEntity shop, string monthStart, string monthEnd)
        {
            try
            {
                string filterQuery = $"&created_at_min={monthStart}&created_at_max={monthEnd}&since_id=0";
                string url = $"https://{shop.ShopID}/admin/api/2022-10/orders/count.json?status=any" + filterQuery;

                // specify store
                _httpClient.DefaultRequestHeaders.Remove("X-Shopify-Access-Token");
                _httpClient.DefaultRequestHeaders.Add("X-Shopify-Access-Token", shop.DataAccessToken);

                var response = await _httpClient.GetAsync(url);
                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var result = JsonConvert.DeserializeObject<CountResponse>(content);
                    return result;
                }
            }
            catch (Exception ex)
            {
                // log error
                await _errorLogger.LogError(ex, OriginSource.ShopifyAPI, DateTime.UtcNow.ToString("yyyy-MM-dd-HH-mm-ss"));
            }

            // could not get order count
            return new CountResponse
            {
                Error = true,
                Message = "Couldn't get Shopify order count."
            };
        }
    }
}
