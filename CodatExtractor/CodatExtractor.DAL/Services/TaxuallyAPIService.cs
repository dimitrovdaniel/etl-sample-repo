using CodatExtractor.DAL.Models;
using CodatExtractor.DAL.Models.Taxually;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace CodatExtractor.DAL.Services
{
    // calls the Taxually API to process data
    public class TaxuallyAPIService
    {
        private string _apiURL;
        private string _apiUser;
        private string _apiPass;
        private HttpClient _httpClient;
        private ErrorLoggingService _errorLogger;
        private string _runTimestamp;

        public TaxuallyAPIService(string apiURL, string username, string password, ErrorLoggingService errorLogger)
        {
            _apiURL = apiURL;
            _apiUser = username;
            _apiPass = password;
            _errorLogger = errorLogger;
            _httpClient = new HttpClient();
            _httpClient.BaseAddress = new Uri(_apiURL);
        }

        // set current run timestamp
        public void SetRunTimestamp(string timestamp)
        {
            _runTimestamp = timestamp;
        }

        // authenticate with the API
        public async Task<bool> Authenticate(string user, string password)
        {
            try
            {
                var jsonBody = JsonConvert.SerializeObject(new TaxTokenRequest
                {
                    GrantType = "password",
                    Email = user,
                    Password = password
                });

                _httpClient.DefaultRequestHeaders.Remove("Authorization");

                var jsonContent = new StringContent(jsonBody, Encoding.UTF8, "application/json");
                var response = await _httpClient.PostAsync("/api/Token/Auth", jsonContent);

                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    var result = JsonConvert.DeserializeObject<TaxTokenResponse>(json);

                    // add token to client headers for future calls
                    if (result.Token != null)
                    {
                        _httpClient.DefaultRequestHeaders.Add("Authorization", "Bearer " + result.Token);
                    }

                    return result.Token != null;
                }
            }
            catch (Exception ex)
            {
                // log error
                await _errorLogger.LogError(ex, OriginSource.CodatAPI, _runTimestamp, "Couldn't authenticate with Taxually API.");
            }

            return false;
        }

        // authenticate for a company by id
        public async Task<bool> AuthenticateForCompany(int companyId)
        {
            try
            {
                var jsonBody = JsonConvert.SerializeObject(new TaxTokenRequest
                {
                    GrantType = "password",
                    Email = _apiURL,
                    Password = _apiPass
                });

                var response = await _httpClient.GetAsync($"/api/Token/ChangeCompany?companyId={companyId}");

                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    var result = JsonConvert.DeserializeObject<TaxTokenResponse>(json);

                    if (result.Token != null)
                    {
                        _httpClient.DefaultRequestHeaders.Remove("Authorization");
                        _httpClient.DefaultRequestHeaders.Add("Authorization", "Bearer " + result.Token);
                    }

                    return result.Token != null;
                }
            }
            catch (Exception ex)
            {
                // log error
                await _errorLogger.LogError(ex, OriginSource.CodatAPI, _runTimestamp, "Couldn't authenticate with Taxually API.");
            }

            return false;
        }

        // get all companies from Taxually
        public async Task<TaxuallyCompanyResponse> GetTaxuallyCompanies()
        {
            try
            {
                var response = await _httpClient.GetAsync("/api/User/Me");

                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    var result = JsonConvert.DeserializeObject<TaxuallyCompanyResponse>(json);

                    return result;
                }
                else
                {
                    // error getting companies
                    var content = await response.Content.ReadAsStringAsync();
                    return new TaxuallyCompanyResponse { Error = true, Message = content };
                }
            }
            catch (Exception ex)
            {
                // log error
                await _errorLogger.LogError(ex, OriginSource.CodatAPI, _runTimestamp);
            }


            // could not get companies
            return new TaxuallyCompanyResponse
            {
                Error = true,
                Message = "Something went wrong."
            };
        }

        // get company periods
        public async Task<Dictionary<string, string>> GetCompanyPeriods()
        {
            try
            {
                var response = await _httpClient.GetAsync("/api/TransactionUpload/ListAvailablePeriods");

                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    var result = JsonConvert.DeserializeObject<Dictionary<string, string>>(json);
                    return result;
                }
            }
            catch (Exception ex)
            {
                // log error
                await _errorLogger.LogError(ex, OriginSource.CodatAPI, _runTimestamp);
            }

            return null;
        }

        // get period details
        public async Task<TaxuallyDataPeriodDetailsResponse> GetDataPeriodDetails(string dataPeriodId)
        {
            try
            {
                var response = await _httpClient.GetAsync($"/api/TransactionUpload/GetDataPeriodDetails?dataPeriodId={dataPeriodId}");

                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    var result = JsonConvert.DeserializeObject<TaxuallyDataPeriodDetailsResponse>(json);
                    return result;
                }
                else
                {
                    // error in getting details
                    var content = await response.Content.ReadAsStringAsync();
                    return new TaxuallyDataPeriodDetailsResponse { Error = true, Message = $"{response.StatusCode.ToString()} {content}" };
                }
            }
            catch (Exception ex)
            {
                // log error
                await _errorLogger.LogError(ex, OriginSource.CodatAPI, _runTimestamp);
            }

            // error in getting details
            return new TaxuallyDataPeriodDetailsResponse
            {
                Error = true,
                Message = "Something went wrong."
            };
        }

        // upload a file to Taxually for processing
        public async Task<StatusMessage> UploadTransactionFile(TaxuallyPeriodBatchRequest request, ProcessPeriodDTO period, string companyName, FileStream csvStream)
        {
            try
            {
                // build file name and create request
                string fileName = $"{companyName.Replace(" ", "-")}_{period.StartDate.ToString("ddMMyyyy")}-{period.EndDate.ToString("ddMMyyyy")}_{DateTime.UtcNow.ToString("ddMMyyyy")}";
                using (var multipartRequest = new MultipartFormDataContent())
                {
                    var fileStreamContent = new StreamContent(csvStream);
                    fileStreamContent.Headers.ContentType = new MediaTypeHeaderValue("text/csv");
                    multipartRequest.Add(fileStreamContent, fileName, $"{fileName}.csv");

                    // perform upload
                    var response = await _httpClient.PostAsync($"/api/TransactionUpload/UploadFile?dataPeriodId={request.DataPeriodID}&uploadBatchId={request.UploadBatchID}", multipartRequest);

                    if (response.IsSuccessStatusCode)
                    {
                        return new StatusMessage();
                    }
                    else
                    {
                        // return response in case of error
                        var content = await response.Content.ReadAsStringAsync();
                        return new StatusMessage { Error = true, Message = content };
                    }
                }
            }
            catch (Exception ex)
            {
                // log error
                await _errorLogger.LogError(ex, OriginSource.CodatAPI, _runTimestamp);
            }

            // error uploading
            return new StatusMessage { Error = true };
        }

        // enqueue uploaded files
        public async Task<StatusMessage> EnqueueUploadedFiles(TaxuallyPeriodBatchRequest request)
        {
            try
            {
                string json = JsonConvert.SerializeObject(request);
                var body = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync($"/api/TransactionUpload/EnqueueUploadedFiles", body);

                if (response.IsSuccessStatusCode)
                {
                    return new StatusMessage();
                }
                else
                {
                    // couldn't complete the request, return response
                    var content = await response.Content.ReadAsStringAsync();
                    return new StatusMessage { Error = true, Message = content };
                }
            }
            catch (Exception ex)
            {
                // log error
                await _errorLogger.LogError(ex, OriginSource.CodatAPI, _runTimestamp);
            }

            return new StatusMessage { Error = true };
        }
    }
}
