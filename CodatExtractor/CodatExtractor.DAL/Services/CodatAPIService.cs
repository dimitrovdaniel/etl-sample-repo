using CodatExtractor.DAL.Models;
using CodatExtractor.DAL.Models.Codat;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace CodatExtractor.DAL.Services
{
    // a service to call the Codat API and manage logic related to it
    public class CodatAPIService
    {
        private string _apikey;
        private HttpClient _httpClient;
        private ErrorLoggingService _errorLogger;
        private string _runTimestamp;

        public CodatAPIService(string apiKey, ErrorLoggingService errorLogger)
        {
            _apikey = apiKey;
            _errorLogger = errorLogger;

            // instantiate http client with Codat API base address and key
            _httpClient = new HttpClient();
            _httpClient.BaseAddress = new Uri("https://api.codat.io");

            var keyBytes = Encoding.UTF8.GetBytes(_apikey);
            _httpClient.DefaultRequestHeaders.Add("Authorization", "Basic " + Convert.ToBase64String(keyBytes));
        }

        // set the current run timestamp
        public void SetRunTimestamp(string timestamp)
        {
            _runTimestamp = timestamp;
        }

        // get all registered companies on Codat
        public async Task<CompanyResponse> GetCompanies()
        {
            try
            {
                var response = await _httpClient.GetAsync("/companies");
                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    var result = JsonConvert.DeserializeObject<CompanyResponse>(json);
                    return result;
                }
            }
            catch (Exception ex)
            {
                // log error
                await _errorLogger.LogError(ex, OriginSource.CodatAPI, _runTimestamp);
            }

            // error getting companies
            return new CompanyResponse
            {
                Error = true,
                Message = "Something went wrong"
            };
        }

        // get company connections by company ID
        public async Task<CompanyConnectionResponse> GetCompanyConnections(string companyId)
        {
            try
            {
                var response = await _httpClient.GetAsync($"/companies/{companyId}/connections");
                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    var result = JsonConvert.DeserializeObject<CompanyConnectionResponse>(json);
                    return result;
                }
            }
            catch (Exception ex)
            {
                // log error
                await _errorLogger.LogError(ex, OriginSource.CodatAPI, _runTimestamp);
            }

            // error getting connections
            return new CompanyConnectionResponse
            {
                Error = true,
                Message = "Something went wrong"
            };
        }

        // get orders for company, connection and date range
        public async Task<CompanyOrderResponse> GetOrdersForConnection(string companyId, string connectionId, string startDate, string endDate)
        {
            try
            {
                var response = await _httpClient.GetAsync($"/companies/{companyId}/connections/{connectionId}/data/commerce-orders?query=createdDate%3E={startDate}%26%26createdDate%3c={endDate}&pageSize=1000");
                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    var result = JsonConvert.DeserializeObject<CompanyOrderResponse>(json);
                    return result;
                }
            }
            catch (Exception ex)
            {
                // log error
                await _errorLogger.LogError(ex, OriginSource.CodatAPI, _runTimestamp);
            }

            // could not get orders
            return new CompanyOrderResponse
            {
                Error = true,
                Message = "Something went wrong"
            };
        }

        // get customer by ID for a company and connection
        public async Task<CompanyCustomer> GetCustomerForConnection(string companyId, string connectionId, string customerId)
        {
            try
            {
                var response = await _httpClient.GetAsync($"/companies/{companyId}/connections/{connectionId}/data/commerce-customers/{customerId}");
                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    var result = JsonConvert.DeserializeObject<CompanyCustomer>(json);
                    return result;
                }
                else
                {
                    // could not get customer, return error with response from API
                    var json = await response.Content.ReadAsStringAsync();
                    return new CompanyCustomer
                    {
                        Error = true,
                        Message = "Failed to get customer response: " + json
                    };
                }
            }
            catch (Exception ex)
            {
                // log error
                await _errorLogger.LogError(ex, OriginSource.CodatAPI, _runTimestamp);
            }

            // couldn't return customer because of exception
            return new CompanyCustomer
            {
                Error = true,
                Message = "Something went wrong"
            };
        }

        // get bills for a company and date range
        public async Task<CompanyBillResponse> GetBillsForCompany(string companyId, string startDate, string endDate)
        {
            try
            {
                // 
                var response = await _httpClient.GetAsync($"/companies/{companyId}/data/bills?query=issueDate%3E={startDate}%26%26issueDate%3c={endDate}&pageSize=1000");
                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    var result = JsonConvert.DeserializeObject<CompanyBillResponse>(json);
                    return result;
                }
                else if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    return new CompanyBillResponse { Results = new List<CompanyBill>() };
                }
            }
            catch (Exception ex)
            {
                // log error
                await _errorLogger.LogError(ex, OriginSource.CodatAPI, _runTimestamp);
            }

            // couldn't get bills
            return new CompanyBillResponse
            {
                Error = true,
                Message = "Something went wrong"
            };
        }

        // get supplier by ID
        public async Task<CompanySupplierResponse> GetSupplier(string companyId, string supplierId)
        {
            try
            {
                var response = await _httpClient.GetAsync($"/companies/{companyId}/data/suppliers/{supplierId}");
                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    var result = JsonConvert.DeserializeObject<CompanySupplierResponse>(json);
                    return result;
                }
            }
            catch (Exception ex)
            {
                // log error
                await _errorLogger.LogError(ex, OriginSource.CodatAPI, _runTimestamp);
            }

            // couldn't get supplier info
            return new CompanySupplierResponse
            {
                Error = true,
                Message = "Something went wrong"
            };
        }

        // get bills credit notes by company id and date range
        public async Task<CompanyBillCreditNoteResponse> GetBillsCreditNotesForCompany(string companyId, string startDate, string endDate)
        {
            try
            {
                var response = await _httpClient.GetAsync($"/companies/{companyId}/data/billCreditNotes?query=issueDate%3E={startDate}%26%26issueDate%3c={endDate}&pageSize=1000");
                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    var result = JsonConvert.DeserializeObject<CompanyBillCreditNoteResponse>(json);
                    return result;
                }
                else if(response.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    // could not find resources
                    return new CompanyBillCreditNoteResponse { Results = new List<CompanyBillCreditNote>() };
                }
            }
            catch (Exception ex)
            {
                // log error
                await _errorLogger.LogError(ex, OriginSource.CodatAPI, _runTimestamp);
            }

            // could not get bills credit notes
            return new CompanyBillCreditNoteResponse
            {
                Error = true,
                Message = "Something went wrong"
            };
        }

        // get invoices for company and date range
        public async Task<CompanyInvoiceResponse> GetInvoicesForCompany(string companyId, string startDate, string endDate)
        {
            try
            {
                var response = await _httpClient.GetAsync($"/companies/{companyId}/data/invoices?query=issueDate%3E={startDate}%26%26issueDate%3c={endDate}&pageSize=1000");
                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    var result = JsonConvert.DeserializeObject<CompanyInvoiceResponse>(json);
                    return result;
                }
                else if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    // invoices not found
                    return new CompanyInvoiceResponse { Results = new List<CompanyInvoice>() };
                }
            }
            catch (Exception ex)
            {
                // log error
                await _errorLogger.LogError(ex, OriginSource.CodatAPI, _runTimestamp);
            }

            // couldn't retrieve invoices
            return new CompanyInvoiceResponse
            {
                Error = true,
                Message = "Something went wrong"
            };
        }

        // get credit notes by company id and date range
        public async Task<CompanyCreditNoteResponse> GetCreditNotesForCompany(string companyId, string startDate, string endDate)
        {
            try
            {
                var response = await _httpClient.GetAsync($"/companies/{companyId}/data/creditNotes?query=issueDate%3E={startDate}%26%26issueDate%3c={endDate}&pageSize=1000");
                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    var result = JsonConvert.DeserializeObject<CompanyCreditNoteResponse>(json);
                    return result;
                }
                else if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    // no credit notes found
                    return new CompanyCreditNoteResponse { Results = new List<CompanyCreditNote>() };
                }
            }
            catch (Exception ex)
            {
                // log error
                await _errorLogger.LogError(ex, OriginSource.CodatAPI, _runTimestamp);
            }

            // couldn't get credit notes
            return new CompanyCreditNoteResponse
            {
                Error = true,
                Message = "Something went wrong"
            };
        }
    }
}
