using CodatExtractor.DAL.Entities.Models;
using CodatExtractor.DAL.Entities;
using CodatExtractor.DAL.Models.Taxually;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CodatExtractor.DAL.Models;
using Microsoft.EntityFrameworkCore;
using Stripe;
using Castle.Core.Configuration;
using CodatExtractor.DAL.Models.Shopify;
using System.Configuration;

namespace CodatExtractor.DAL.Services
{
    // the services that provides functionality for the admin website
    // it returns mapping and log data
    public class AdminService
    {
        private COEXTRContext _context;
        private ErrorLoggingService _errorLogger;
        private UserInfo _activeUser;
        private StripeAPIService _stripeService;

        public AdminService(COEXTRContext db, ErrorLoggingService errorLogger, StripeAPIService stripeService)
        {
            _context = db;
            _errorLogger = errorLogger;
            _stripeService = stripeService;
        }

        // set the current user
        public void SetActiveUser(UserInfo user)
        {
            _activeUser = user;
        }

        // perform login
        public async Task<UserInfo> Login(LoginInfo data)
        {
            try
            {
                // check the provided user & pass against the user account records in DB
                var account = _context.UserAccounts.FirstOrDefault(x => x.Username == data.Username && x.PasswordPlain == data.Password);
                if(account != null)
                {
                    _activeUser = new UserInfo
                    {
                        Username = account.Username
                    };
                    return _activeUser;
                }
            }
            catch (Exception ex)
            {
                // log error
                await _errorLogger.LogError(ex, OriginSource.ExtractorApp, "Admin", "User failed to login.");
            }

            // login invalid, return error
            return new UserInfo
            {
                Error = true,
                Message = "Invalid username/password."
            };
        }

        // get company mappings (Codat)
        public async Task<List<CompanyMapping>> GetCompanyMappings()
        {
            try
            {
                // get mappings from DB table
                var mappings = _context.CompanyMappings.OrderBy(x => x.DateCreated).ToList();

                return mappings.Select(x => new CompanyMapping
                {
                    Company = x.CompanyName,
                    SourceCompanyID = x.CodatCompanyID,
                    TaxuallyCompanyID = x.TaxuallyCompanyID
                }).ToList();
            }
            catch (Exception ex)
            {
                // log error
                await _errorLogger.LogError(ex, OriginSource.ExtractorApp, "Admin", "Failed to get company mappings.");
            }

            return null;
        }
        
        // get company mappings for Shopify
        public async Task<List<CompanyMapping>> GetShopifyMappings()
        {
            try
            {
                // get registered Shopify stores
                var stores = _context.ShopifyShops.ToList();
                // get Shopify mappings
                var mappings = _context.ShopifyMappings.OrderBy(x => x.DateCreated).ToList();

                // create result mappings list
                var results = mappings.Select(x => new CompanyMapping
                {
                    Company = x.CompanyName,
                    SourceCompanyID = x.ShopifyShopID,
                    TaxuallyCompanyID = x.TaxuallyCompanyID
                }).ToList();

                return results;
            }
            catch (Exception ex)
            {
                // log error
                await _errorLogger.LogError(ex, OriginSource.ExtractorApp, "Admin", "Failed to get Shopify company mappings.");
            }

            return null;
        }

        // return the registered Shopify stores
        public async Task<List<ShopifyStore>> GetShopifyStores()
        {
            try
            {
                // return Shopify store names from DB
                return _context.ShopifyShops.Where(x => !x.ShopifyMappings.Any()).Select(x => new ShopifyStore
                {
                    Name = x.ShopID.ToString()
                }).ToList();
            }
            catch (Exception ex)
            {
                // log error
                await _errorLogger.LogError(ex, OriginSource.StripeAPI, "Admin", "Failed to get Stripe API accounts.");
            }

            return null;
        }
        
        // get Stripe accounts
        public async Task<StripeList<Account>> GetStripeAccounts()
        {
            try
            {
                // get linked accounts on Stripe from their API
                return _stripeService.GetAccounts();
            }
            catch (Exception ex)
            {
                // log error
                await _errorLogger.LogError(ex, OriginSource.StripeAPI, "Admin", "Failed to get Stripe API accounts.");
            }

            return null;
        }

        // get Stripe mappings from DB
        public async Task<List<CompanyMapping>> GetStripeMappings()
        {
            try
            {
                // get Stripe mappings from DB
                var mappings = _context.StripeMappings.OrderBy(x => x.DateCreated).ToList();

                var results = mappings.Select(x => new CompanyMapping
                {
                    Company = x.CompanyName,
                    SourceCompanyID = x.StripeAccountId,
                    TaxuallyCompanyID = x.TaxuallyCompanyID
                }).ToList();

                return results;
            }
            catch (Exception ex)
            {
                // log error
                await _errorLogger.LogError(ex, OriginSource.StripeAPI, "Admin", "Failed to get Stripe company mappings.");
            }

            return null;
        }

        // save modified company mappings by admin
        public async Task<StatusMessage> SaveCompanyMappings(List<CompanyMapping> mappings)
        {
            try
            {
                var dbMappings = _context.CompanyMappings.ToList();

                // add new mappings
                if (mappings.Any(x => !dbMappings.Any(db => db.TaxuallyCompanyID == x.TaxuallyCompanyID && db.CodatCompanyID == x.SourceCompanyID)))
                    _context.CompanyMappings.AddRange(mappings.Where(x => !dbMappings
                        .Any(db => db.TaxuallyCompanyID == x.TaxuallyCompanyID && db.CodatCompanyID == x.SourceCompanyID))
                        .Select(x => new CompanyMappingEntity
                        {
                            CodatCompanyID = x.SourceCompanyID,
                            CompanyName = x.Company,
                            CreatedByUser = _activeUser.Username,
                            DateCreated = DateTime.UtcNow,
                            TaxuallyCompanyID = x.TaxuallyCompanyID
                        }).ToList());

                // delete removed mappings
                _context.CompanyMappings.RemoveRange(dbMappings.Where(x => !mappings.Any(m => m.TaxuallyCompanyID == x.TaxuallyCompanyID && m.SourceCompanyID == x.CodatCompanyID)).AsQueryable());
                await _context.SaveChangesAsync();

                return new StatusMessage();
            }
            catch (Exception ex)
            {
                // log error
                await _errorLogger.LogError(ex, OriginSource.ExtractorApp, "Admin", "Failed to save company mappings.");
            }

            // something went wrong
            return new StatusMessage
            {
                Error = true,
                Message = "Something went wrong."
            };
        }

        // save modified Shopify company mappings from Admin
        public async Task<StatusMessage> SaveShopifyCompanyMappings(List<CompanyMapping> mappings)
        {
            try
            {
                var dbMappings = _context.ShopifyMappings.ToList();

                // add new mappings
                if (mappings.Any(x => !dbMappings.Any(db => db.TaxuallyCompanyID == x.TaxuallyCompanyID && db.ShopifyShopID == x.SourceCompanyID)))
                    _context.ShopifyMappings.AddRange(mappings.Where(x => !dbMappings
                        .Any(db => db.TaxuallyCompanyID == x.TaxuallyCompanyID && db.ShopifyShopID == x.SourceCompanyID))
                        .Select(x => new ShopifyMappingEntity
                        {
                            ShopifyShopID = x.SourceCompanyID,
                            CompanyName = x.Company,
                            CreatedByUser = _activeUser.Username,
                            DateCreated = DateTime.UtcNow,
                            TaxuallyCompanyID = x.TaxuallyCompanyID
                        }).ToList());

                // delete removed mappings
                _context.ShopifyMappings.RemoveRange(dbMappings.Where(x => !mappings.Any(m => m.TaxuallyCompanyID == x.TaxuallyCompanyID && m.SourceCompanyID == x.ShopifyShopID)).AsQueryable());
                await _context.SaveChangesAsync();

                return new StatusMessage();
            }
            catch (Exception ex)
            {
                // log error
                await _errorLogger.LogError(ex, OriginSource.ExtractorApp, "Admin", "Failed to save Shopify company mappings.");
            }

            // something went wrong
            return new StatusMessage
            {
                Error = true,
                Message = "Something went wrong."
            };
        }
        
        // save modified Stripe company mappings from admin
        public async Task<StatusMessage> SaveStripeCompanyMappings(List<CompanyMapping> mappings)
        {
            try
            {
                var dbMappings = _context.StripeMappings.ToList();

                // add new mappings
                if (mappings.Any(x => !dbMappings.Any(db => db.TaxuallyCompanyID == x.TaxuallyCompanyID && db.StripeAccountId == x.SourceCompanyID)))
                    _context.StripeMappings.AddRange(mappings.Where(x => !dbMappings
                        .Any(db => db.TaxuallyCompanyID == x.TaxuallyCompanyID && db.StripeAccountId == x.SourceCompanyID))
                        .Select(x => new StripeMappingEntity
                        {
                            StripeAccountId = x.SourceCompanyID,
                            CompanyName = x.Company,
                            CreatedByUser = _activeUser.Username,
                            DateCreated = DateTime.UtcNow,
                            TaxuallyCompanyID = x.TaxuallyCompanyID
                        }).ToList());

                // delete removed mappings
                _context.StripeMappings.RemoveRange(dbMappings.Where(x => !mappings.Any(m => m.TaxuallyCompanyID == x.TaxuallyCompanyID && m.SourceCompanyID == x.StripeAccountId)).AsQueryable());
                await _context.SaveChangesAsync();

                return new StatusMessage();
            }
            catch (Exception ex)
            {
                // log error
                await _errorLogger.LogError(ex, OriginSource.StripeAPI, "Admin", "Failed to save Stripe company mappings.");
            }

            // something went wrong
            return new StatusMessage
            {
                Error = true,
                Message = "Something went wrong."
            };
        }

        // return log details of runs
        public async Task<RunDetailsResponse> GetRunLogs()
        {
            try
            {
                var result = new RunDetailsResponse();

                // EF tends to cache results in a single thread, dispose context and recreate
                string cs = _context.DisposeAndRefresh();
                _context = new COEXTRContext(cs);

                // get all available mappings
                var codatMappings = _context.CompanyMappings.ToList();
                var shopifyMappings = _context.ShopifyMappings.ToList();
                var allMappings = codatMappings.Select(x => new { x.CompanyName, x.TaxuallyCompanyID })
                    .Union(shopifyMappings.Select(x => new { x.CompanyName, x.TaxuallyCompanyID })).ToList();

                var allIds = allMappings.Select(x => x.TaxuallyCompanyID).Distinct().ToArray();

                var lastCompanyRunData = _context.RunCompanyPeriods.Where(x => allIds.Contains(x.TaxuallyCompanyID) && x.PeriodStatus > -1 && x.TotalOrdersPulled > -1).OrderByDescending(x => x.RunTimestamp)
                    .ToList();

                // get last 10 run IDs
                var runLogs = _context.Runs.OrderByDescending(x => x.RunTimestamp).Take(10).ToList();
                string[] timestamps = runLogs.Select(x => x.RunTimestamp).ToArray();

                // get error logs for runs
                var errorLogs = _context.ErrorLogs.Where(x => timestamps.Contains(x.RunTimestamp)).OrderByDescending(x => x.DateCreated).ToList();

                result.CompanyRunResults = new List<CompanyRunResults>();

                // build log results for each mapping
                foreach (var companyId in allIds)
                {
                    var data = lastCompanyRunData
                        .Where(x => x.TaxuallyCompanyID == companyId && x.PeriodStatus > -1 && x.TotalOrdersPulled > -1)
                        .OrderByDescending(x => x.RunTimestamp)
                        .ThenByDescending(x => x.PeriodDate)
                        .FirstOrDefault();

                    if (data == null)
                    {
                        // no data for this company, add an empty object
                        result.CompanyRunResults.Add(new CompanyRunResults
                        {
                            CompanyName = allMappings.Where(x => x.TaxuallyCompanyID == companyId).Select(x => x.CompanyName).FirstOrDefault()
                        });
                        continue;
                    }

                    var lastRunTime = data.RunTimestamp.Split('-');

                    // add company run data
                    result.CompanyRunResults.Add(new CompanyRunResults
                    {
                        CompanyName = data.CompanyName,
                        LastActiveRun = lastRunTime[2] + "/" + lastRunTime[1] + "/" + lastRunTime[0],
                        Period = data.PeriodDate.Split('T')[0],
                        Status = data.PeriodStatus.ToString(),
                        ItemsPulled = "Orders: " + data.TotalOrdersPulled 
                            + (data.TotalCreditNotesPulled > 0 ? "; Credit Notes: " + data.TotalCreditNotesPulled : "")
                            + (data.TotalBillsPulled > 0 ? "; Bills: " + data.TotalBillsPulled: "")
                            + (data.TotalInvoicesPulled > 0 ? "; Invoices: " + data.TotalInvoicesPulled: "")
                            + (data.TotalBillCreditNotesPulled > 0 ? "; Bill Credit Notes: " + data.TotalBillCreditNotesPulled: ""),
                    });
                }

                // return the last 10 runs
                result.Last10Results = runLogs.Select(x => new RunDetails
                {
                    RunTimestamp = x.RunTimestamp,
                    InProgress = x.InProgress || (!x.InProgress && !x.IsCompleted),
                    InvokedByUser = x.InvokedByUser,
                    IsCompleted = x.IsCompleted,
                    WasSuccessful = x.WasSuccessful,
                    WasUserInvoked = x.WasUserInvoked,
                    RunDetailsRecords = x.RunCompanyPeriods.Where(r => r.PeriodStatus > -1).Select(r => new RunDetailsRecord
                    {
                        ProcessingStartedAt = r.ProcessingStartedAt,
                        IsPeriodProcessed = r.IsPeriodProcessed ?? false,
                        CompanyName = r.CompanyName,
                        CSVText = x.RunCSVs.Where(p => p.PeriodID == r.PeriodID).Select(p => p.CSVText).FirstOrDefault(),
                        IsEnqueuedOnTaxually = x.RunCSVs.Where(p => p.PeriodID == r.PeriodID).Select(p => p.IsEnqueuedOnTaxually).FirstOrDefault(),
                        IsUploadedToTaxually = x.RunCSVs.Where(p => p.PeriodID == r.PeriodID).Select(p => p.IsUploadedToTaxually).FirstOrDefault(),
                        PeriodRange = r.PeriodDate.Split('T')[0],
                        TaxuallyCompanyID = r.TaxuallyCompanyID,
                        TotalOrdersPulled = r.TotalOrdersPulled,
                        TotalBillsPulled = r.TotalBillsPulled ?? 0,
                        TotalInvoicesPulled = r.TotalInvoicesPulled ?? 0,
                        TotalBillCreditNotesPulled = r.TotalBillCreditNotesPulled ?? 0,
                        TotalCreditNotesPulled = r.TotalCreditNotesPulled ?? 0,
                        PeriodStatus = r.PeriodStatus
                    }).OrderBy(x => x.PeriodRange).ToList(),
                    RunErrors = errorLogs.Where(e => e.RunTimestamp == x.RunTimestamp).Select(e => new RunErrorRecord
                    {
                        DateCreated = e.DateCreated,
                        ErrorMessage = e.ErrorMessage,
                        RawException = e.RawException,
                        Source = e.OriginSource
                    }).ToList()
                }).ToList();

                return result;
            }
            catch (Exception ex)
            {
                // log error
                await _errorLogger.LogError(ex, OriginSource.ExtractorApp, "Admin", "Failed to get company mappings.");
            }

            return null;
        }

        // trigger a manual run on the extractor app
        public async Task ManualRun(int? runForCompany, string runForPeriod = null)
        {
            var executionTime = DateTime.UtcNow;
            string runTimestamp = executionTime.ToString("yyyy-MM-dd-HH-mm-ss");

            // add run info to DB to be picked up by the AF
            _context.Runs.Add(new RunEntity
            {
                DateExecuted = executionTime,
                RunTimestamp = runTimestamp,
                InProgress = false,
                InvokedByUser = _activeUser.Username,
                WasUserInvoked = true,
                RunCompanyPeriods = new List<RunCompanyPeriodEntity>(),
                RunForCompany = runForCompany, // populate with company ID if selected
                RunForPeriod = runForPeriod
            });

            await _context.SaveChangesAsync();
        }

        // get all scheduled runs
        public async Task<List<ScheduledRunInfo>> GetScheduledRuns()
        {
            return _context.ScheduledRuns.Select(x => new ScheduledRunInfo
            {
                ID = x.ID,
                IntervalType = x.RunIntervalType,
                RunTime = x.RunTime
            }).ToList();
        }

        // save modified scheduled run settings
        public async Task<StatusMessage> SaveScheduledRuns(List<ScheduledRunInfo> data)
        {
            var dbRuns = _context.ScheduledRuns.ToList();

            // save new and modified
            foreach(var item in data)
            {
                var existing = dbRuns.FirstOrDefault(x => x.ID == item.ID);
                
                if(existing != null)
                {
                    existing.RunIntervalType = item.IntervalType;
                    existing.RunTime = item.RunTime;
                }
                else
                {
                    _context.ScheduledRuns.Add(new ScheduledRunEntity
                    {
                        RunTime = item.RunTime,
                        RunIntervalType = item.IntervalType
                    });
                }
            }

            // delete removed runs
            _context.ScheduledRuns.RemoveRange(dbRuns.Where(x => !data.Any(d => d.ID == x.ID)).AsQueryable());

            await _context.SaveChangesAsync();
            return new StatusMessage();
        }

        // get taxually credential data from DB
        public async Task<List<TaxuallyCred>> GetCredentials()
        {
            return _context.TaxuallyCredentials
                .Select(x => new TaxuallyCred
                {
                    User = x.TaxuallyUser,
                    Password = x.TaxuallyPassword
                }).ToList(); ;
        }

        // save modified credentials
        public async Task<StatusMessage> SaveCredentials(List<TaxuallyCred> data)
        {
            var savedCredentials = _context.TaxuallyCredentials.ToList();

            // save new/modified credentials
            foreach(var item in data)
            {
                var existing = savedCredentials.FirstOrDefault(x => item.User == x.TaxuallyUser);
                
                if(existing != null)
                {
                    existing.TaxuallyPassword = item.Password;
                }
                else
                {
                    _context.TaxuallyCredentials.Add(new TaxuallyCredentials
                    {
                        TaxuallyUser = item.User,
                        TaxuallyPassword = item.Password
                    });
                }
            }

            // remove deleted credentials
            _context.TaxuallyCredentials.RemoveRange(savedCredentials.Where(x => !data.Any(d => d.User == x.TaxuallyUser)).AsQueryable());
            await _context.SaveChangesAsync();

            return new StatusMessage();
        }
    }
}
