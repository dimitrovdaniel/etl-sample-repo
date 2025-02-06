using Castle.Core.Resource;
using CodatExtractor.DAL.Entities;
using CodatExtractor.DAL.Entities.Models;
using CodatExtractor.DAL.Models;
using CodatExtractor.DAL.Models.Codat;
using CodatExtractor.DAL.Models.Shopify;
using CodatExtractor.DAL.Models.Stripe;
using CodatExtractor.DAL.Models.Taxually;
using CodatExtractor.DAL.Services.PeriodProcessors;
using Microsoft.Azure.Amqp.Encoding;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion.Internal;
using Microsoft.EntityFrameworkCore.Update;
using Microsoft.Extensions.Azure;
using Microsoft.Identity.Client;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.ExceptionServices;
using System.Text;
using System.Threading.Tasks;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

namespace CodatExtractor.DAL.Services
{
    // the main logic resides in this service
    // it controls run logic and gets data from provider services
    public class RunManagementService
    {
        private COEXTRContext _context;
        private CodatAPIService _codatService;
        private ShopifyAPIService _shopifyService;
        private TaxuallyAPIService _taxService;
        private ServiceBusService _serviceBusService;
        private StripeAPIService _stripeService;
        private ErrorLoggingService _errorLogger;
        private string _runTimestamp;
        private string _tempCSVLoc;
        private List<TaxuallyCompanyWithPeriods> _targetCompanies;
        private RunEntity? _currentRun;
        private List<ShopifyMappingEntity> _shopifyMappings;
        private List<StripeMappingEntity> _stripeMappings;
        private List<CompanyMappingEntity> _codatMappings;

        // inject all needed services
        public RunManagementService(COEXTRContext db, CodatAPIService codatService, TaxuallyAPIService taxService, ErrorLoggingService errorLogger, ShopifyAPIService shopifyService,
            ServiceBusService serviceBusService, StripeAPIService stripeService)
        {
            _context = db;
            _codatService = codatService;
            _shopifyService = shopifyService;
            _errorLogger = errorLogger;
            _taxService = taxService;
            _serviceBusService = serviceBusService;
            _stripeService = stripeService;
        }

        // perform manual run. This method is primarily used by the test console app
        public async Task ManualRun()
        {
            // set run timestamp for all services
            var executionTime = DateTime.UtcNow;
            _runTimestamp = executionTime.ToString("yyyy-MM-dd-HH-mm-ss");
            _codatService.SetRunTimestamp(_runTimestamp);
            _taxService.SetRunTimestamp(_runTimestamp);

            // record run in DB to be picked up by AF
            _context.Runs.Add(new RunEntity
            {
                DateExecuted = executionTime,
                RunTimestamp = _runTimestamp,
                InProgress = false,
                InvokedByUser = "Codat test",
                WasUserInvoked = true,
                RunCompanyPeriods = new List<RunCompanyPeriodEntity>()
            });

            await _context.SaveChangesAsync();
        }

        // [NO LONGER NEEDED] get run info, used by the WinForms test app
        public async Task<RunEntity> GetRunInfo()
        {
            return _context.Runs.AsNoTracking().Include(x => x.RunCSVs).Include(x => x.RunCompanyPeriods).FirstOrDefault(x => x.RunTimestamp == _runTimestamp);
        }

        // checks the scheduled runs settings to determine if a run should be performed at this point in time
        public async Task CheckAndInsertScheduledRuns()
        {
            var time = DateTime.UtcNow;
            var scheduledRuns = _context.ScheduledRuns.ToList();

            bool matchedForRun = false;
            bool isDaily = false;

            // iterate the scheduled runs from DB
            foreach (var run in scheduledRuns)
            {
                // run should be ran daily, check if it matches the time
                if (run.RunIntervalType == "daily")
                {
                    string[] timeData = run.RunTime.Split(':');
                    if (timeData.Length == 2 && int.TryParse(timeData[0], out _) && int.Parse(timeData[0]) == time.Hour
                        && int.TryParse(timeData[1], out _) && int.Parse(timeData[1]) == time.Minute)
                    {
                        // if matched for run don't check anymore, no need to run twice
                        matchedForRun = true;
                        isDaily = true;
                        break;
                    }
                }
                // run should be ran monthly, check if it matches the date
                else if (run.RunIntervalType == "monthly")
                {
                    if (int.TryParse(run.RunTime, out _) && int.Parse(run.RunTime) == time.Day)
                    {
                        matchedForRun = true;
                        break;
                    }
                }
            }

            // if a run is matched, create a new run in DB to be picked up by the AF
            if (matchedForRun)
            {
                string timestampPartial = isDaily ? time.ToString("yyyy-MM-dd-HH-mm") : time.ToString("yyyy-MM-dd");
                string timestamp = time.ToString("yyyy-MM-dd-HH-mm-ss");
                var alreadyRan = _context.Runs.FirstOrDefault(x => x.RunTimestamp.StartsWith(timestampPartial));

                // no run at that time yet, run now
                if (alreadyRan == null)
                {
                    _context.Runs.Add(new RunEntity
                    {
                        DateExecuted = time,
                        RunTimestamp = timestamp,
                        InProgress = false,
                        InvokedByUser = "Scheduled",
                        WasUserInvoked = false,
                        RunCompanyPeriods = new List<RunCompanyPeriodEntity>()
                    });

                    await _context.SaveChangesAsync();
                }
            }
        }

        // get the next run to perform (scheduled or manual)
        public async Task CheckAndRunLogic()
        {
            var targetRun = _context.Runs.Where(x => !x.IsCompleted && !x.InProgress).OrderByDescending(x => x.DateExecuted).FirstOrDefault();

            // if a run exists, initiate and get needed data
            if (targetRun != null)
            {
                await InitiateExistingRun(targetRun);
                await ObtainTaxuallyCompanies();
                await ObtainAndPreparePeriods();
            }
        }

        // start an existing run
        public async Task InitiateExistingRun(RunEntity run)
        {
            // set service timestamps
            _runTimestamp = run.RunTimestamp;
            _tempCSVLoc = Path.GetTempPath();
            _codatService.SetRunTimestamp(_runTimestamp);
            _taxService.SetRunTimestamp(_runTimestamp);

            try
            {
                // set run in progress
                run.InProgress = true;
                run.DateExecuted = DateTime.UtcNow;
                run.RunCompanyPeriods = new List<RunCompanyPeriodEntity>();

                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                // log error
                await _errorLogger.LogError(ex, OriginSource.ExtractorApp, _runTimestamp, "Couldn't instantiate RMS.");
            }
        }

        // start and create a new run
        public async Task InitiateRun()
        {
            // set service timestamps
            var executionTime = DateTime.UtcNow;
            _runTimestamp = executionTime.ToString("yyyy-MM-dd-HH-mm-ss");
            _tempCSVLoc = Path.GetTempPath();
            _codatService.SetRunTimestamp(_runTimestamp);
            _taxService.SetRunTimestamp(_runTimestamp);

            try
            {
                // record run to be picked up
                _context.Runs.Add(new RunEntity
                {
                    DateExecuted = executionTime,
                    RunTimestamp = _runTimestamp,
                    InProgress = true,
                    RunCompanyPeriods = new List<RunCompanyPeriodEntity>(),
                    RunForCompany= 10062,
                    RunForPeriod = "2023-04-01"
                });

                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                // log error
                await _errorLogger.LogError(ex, OriginSource.ExtractorApp, _runTimestamp, "Couldn't instantiate RMS.");
            }
        }

        // get taxually company data to be used in mappings
        public async Task ObtainTaxuallyCompanies()
        {
            try
            {
                // get all mappings
                _shopifyMappings = _context.ShopifyMappings.ToList();
                _stripeMappings = _context.StripeMappings.ToList();
                _codatMappings = _context.CompanyMappings.ToList();

                // get all taxually credentials
                var taxCreds = _context.TaxuallyCredentials.ToList();

                _currentRun = _context.Runs.FirstOrDefault(x => x.RunTimestamp == _runTimestamp);
                _targetCompanies = new List<TaxuallyCompanyWithPeriods>();

                // iterate credentials to get all available companies
                foreach (var cred in taxCreds)
                {
                    // authenticate with credential
                    var isAuthenticated = await _taxService.Authenticate(cred.TaxuallyUser, cred.TaxuallyPassword);

                    // couldn't authenticate - throw error
                    if (!isAuthenticated)
                    {
                        var run = _context.Runs.FirstOrDefault(x => x.RunTimestamp == _runTimestamp);
                        run.IsCompleted = true;
                        run.WasSuccessful = false;
                        run.InProgress = false;

                        await _context.SaveChangesAsync();
                        await _errorLogger.LogError(null, OriginSource.ExtractorApp, _runTimestamp, "Couldn't obtain Taxually companies.");
                        return;
                    }

                    // get companies
                    var taxCompanies = await _taxService.GetTaxuallyCompanies();

                    // error getting companies - stop
                    if(taxCompanies.Error || taxCompanies.Companies == null)
                    {
                        await _errorLogger.LogError(new Exception(taxCompanies.Message), OriginSource.TaxuallyAPI, _runTimestamp, "Couldn't obtain Taxually companies.");
                        continue;
                    }

                    var periodStart = DateTime.Now.AddMonths(-2).AddDays(-DateTime.Now.Day + 1);

                    // create the list of taxually companies and their matching mapping IDs
                    foreach (var company in taxCompanies.Companies.Where(x => (_currentRun.RunForCompany == null || x.Id == _currentRun.RunForCompany) && (_codatMappings.Any(c => c.TaxuallyCompanyID == x.Id) || _shopifyMappings.Any(c => c.TaxuallyCompanyID == x.Id) || _stripeMappings.Any(c => c.TaxuallyCompanyID == x.Id))))
                    {
                        var changeCompany = await _taxService.AuthenticateForCompany(company.Id);

                        // get company periods in case authentication is successful
                        if (changeCompany)
                        {
                            var periods = await _taxService.GetCompanyPeriods();

                            if (periods != null)
                            {
                                var companyWithPeriods = new TaxuallyCompanyWithPeriods
                                {
                                    BatchId = Guid.NewGuid(),
                                    CompanyId = company.Id,
                                    CompanyName = company.LegalNameOfBusiness,
                                    Periods = new List<TaxuallyPeriod>(),
                                    TaxuallyUser = cred.TaxuallyUser,
                                    TaxuallyPassword = cred.TaxuallyPassword
                                };

                                // check periods for eligibility and add to the list
                                foreach (var period in periods)
                                {
                                    string startDate = period.Value.Split('T')[0];
                                    DateTime parsedDate = DateTime.ParseExact(startDate, "yyyy-MM-dd", null);

                                    if (_currentRun.RunForCompany == null 
                                            && (parsedDate < periodStart.Date || (parsedDate.Year == DateTime.Today.Year && parsedDate.Month == DateTime.Today.Month)))
                                        continue; // only take data for the last two months

                                    // run is for a single period of a company, skip periods that don't match
                                    if (_currentRun.RunForCompany != null && _currentRun.RunForPeriod != null && _currentRun.RunForPeriod != startDate)
                                        continue;

                                    companyWithPeriods.Periods.Add(new TaxuallyPeriod
                                    {
                                        Id = period.Key,
                                        Date = period.Value
                                    });
                                }

                                _targetCompanies.Add(companyWithPeriods);
                            }
                            else
                            {
                                // error getting periods
                                await _errorLogger.LogError(null, OriginSource.TaxuallyAPI, _runTimestamp, $"Failed to list available periods for company ({company.Id}) {company.LegalNameOfBusiness}.");
                            }
                        }
                        else
                        {
                            // error authenticating company
                            await _errorLogger.LogError(null, OriginSource.TaxuallyAPI, _runTimestamp, $"Failed to authenticate for company ({company.Id}) {company.LegalNameOfBusiness}.");
                        }
                    }
                }


                // record target companies in run
                var updateRun = _context.Runs.FirstOrDefault(x => x.RunTimestamp == _runTimestamp);

                if (updateRun.RunCompanyPeriods == null)
                    updateRun.RunCompanyPeriods = new List<RunCompanyPeriodEntity>();

                foreach (var company in _targetCompanies)
                {
                    foreach (var x in company.Periods)
                        updateRun.RunCompanyPeriods.Add(new RunCompanyPeriodEntity
                        {
                            TaxuallyCompanyID = company.CompanyId,
                            CompanyName = company.CompanyName,
                            PeriodID = x.Id,
                            PeriodDate = x.Date,
                            PeriodStatus = -1,
                            RunTimestamp = _runTimestamp,
                            TotalOrdersPulled = -1
                        });
                }

                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                // log error
                await _errorLogger.LogError(ex, OriginSource.ExtractorApp, _runTimestamp, $"Failed to get Taxually data.");

                // record error in run
                var updateRun = _context.Runs.FirstOrDefault(x => x.RunTimestamp == _runTimestamp);
                updateRun.WasSuccessful = false;
                updateRun.IsCompleted = true;
                updateRun.InProgress = false;
                await _context.SaveChangesAsync();
            }
        }

        // prepare periods to be processed by AF
        public async Task ObtainAndPreparePeriods(bool fromConsole = false)
        {
            try
            {
                // get run data
                _currentRun = _context.Runs.FirstOrDefault(x => x.RunTimestamp == _runTimestamp);

                var periodStart = DateTime.Now.AddMonths(-2).AddDays(-DateTime.Now.Day + 1);
                _serviceBusService.InitServiceBus();

                // iterate through available companies and mappings
                foreach (var company in _targetCompanies.Where(x => _shopifyMappings.Any(s => s.TaxuallyCompanyID == x.CompanyId) || _codatMappings.Any(c => c.TaxuallyCompanyID == x.CompanyId) 
                    || _stripeMappings.Any(c => c.TaxuallyCompanyID == x.CompanyId)))
                {
                    // authenticate with Taxually
                    var authenticate = await _taxService.Authenticate(company.TaxuallyUser, company.TaxuallyPassword);
                    if (!authenticate)
                    {
                        // couldn't authenticate
                        await _errorLogger.LogError(null, OriginSource.TaxuallyAPI, _runTimestamp, $"Failed to authenticate user {company.TaxuallyUser} for company ({company.CompanyId}) {company.CompanyName}.");
                        continue;
                    }

                    // authenticate for company
                    var changeCompany = await _taxService.AuthenticateForCompany(company.CompanyId);

                    if (!changeCompany)
                    {
                        // couldn't authenticate
                        await _errorLogger.LogError(null, OriginSource.TaxuallyAPI, _runTimestamp, $"Failed to authenticate for company ({company.CompanyId}) {company.CompanyName}.");
                        continue;
                    }

                    // go through each period and send to AF for processing
                    foreach (var period in company.Periods)
                    {
                        string startDate = period.Date.Split('T')[0];
                        DateTime parsedDate = DateTime.ParseExact(startDate, "yyyy-MM-dd", null);
                        string endDate = parsedDate.AddMonths(1).AddDays(-1).ToString("yyyy-MM-dd");

                        if (_currentRun.RunForCompany == null && (parsedDate < periodStart.Date || (parsedDate.Year == DateTime.Today.Year && parsedDate.Month == DateTime.Today.Month)))
                        {
                            continue; // only take data for the last two months
                        }


                        // run is for a single period of a company, skip periods that don't match
                        if (_currentRun.RunForCompany != null && _currentRun.RunForPeriod != null && _currentRun.RunForPeriod != startDate)
                            continue;

                        period.StartDate = parsedDate;
                        period.EndDate = parsedDate.AddMonths(1).AddDays(-1);

                        // get period details from Taxuallly
                        var periodDetails = await _taxService.GetDataPeriodDetails(period.Id);

                        if (periodDetails.Error)
                        {
                            // log error and continue
                            await _errorLogger.LogError(new Exception(periodDetails.Message), OriginSource.TaxuallyAPI, _runTimestamp, $"Failed to get Taxually Data Period Details for company ({company.CompanyId}) {company.CompanyName} and period {startDate} - {endDate}. Error: {periodDetails.Message}.");
                            continue;
                        }

                        string periodFileName = $"{company.CompanyName.Replace(" ", "-")}_{period.StartDate.ToString("ddMMyyyy")}-{period.EndDate.ToString("ddMMyyyy")}";

                        var dbPeriod = _currentRun.RunCompanyPeriods.FirstOrDefault(x => x.PeriodID == period.Id);

                        if (dbPeriod != null && !periodDetails.Error)
                            dbPeriod.PeriodStatus = periodDetails.Status;

                        await _context.SaveChangesAsync();

                        // has existing file uploaded for period
                        if (periodDetails.Status == 60 || periodDetails.UploadedFiles.Any(x => x.Name.StartsWith(periodFileName)))
                        {
                            period.IsProcessed = true;
                            dbPeriod.IsPeriodProcessed = true;
                            await _context.SaveChangesAsync();
                            continue;
                        }

                        // running in debug mode / from console. Call the process period directly (do not invoke AF processing)
                        if (fromConsole)
                            await ProcessPeriod(new ProcessPeriodDTO
                            {
                                CompanyBatchId = company.BatchId,
                                CompanyId = company.CompanyId,
                                CompanyName = company.CompanyName,
                                EndDate = period.EndDate,
                                PeriodId = period.Id,
                                RunTimestamp = _runTimestamp,
                                StartDate = period.StartDate,
                                TaxuallyUser = company.TaxuallyUser,
                                TaxuallyPassword = company.TaxuallyPassword,
                                CodatCompanyId = _codatMappings.Where(x => x.TaxuallyCompanyID == company.CompanyId)
                                    .Select(x => x.CodatCompanyID).FirstOrDefault(),
                                StripeAccountId = _stripeMappings.Where(x => x.TaxuallyCompanyID == company.CompanyId)
                                    .Select(x => x.StripeAccountId).FirstOrDefault()
                            });
                        // send through service bus to the Azure Functions for processing
                        // each period is processed by a different instance of an Azure Function which allows rapid processing of large data
                        else
                            await _serviceBusService.SendMessage(JsonConvert.SerializeObject(new ProcessPeriodDTO
                            {
                                CompanyBatchId = company.BatchId,
                                CompanyId = company.CompanyId,
                                CompanyName = company.CompanyName,
                                EndDate = period.EndDate,
                                PeriodId = period.Id,
                                RunTimestamp = _runTimestamp,
                                StartDate = period.StartDate,
                                TaxuallyUser = company.TaxuallyUser,
                                TaxuallyPassword = company.TaxuallyPassword,
                                CodatCompanyId = _codatMappings.Where(x => x.TaxuallyCompanyID == company.CompanyId)
                                    .Select(x => x.CodatCompanyID).FirstOrDefault(),
                                StripeAccountId = _stripeMappings.Where(x => x.TaxuallyCompanyID == company.CompanyId)
                                    .Select(x => x.StripeAccountId).FirstOrDefault()
                            }));
                    }
                }

                // if all period are processed / do not need processing, end run
                if (_targetCompanies.Where(x => _shopifyMappings.Any(s => s.TaxuallyCompanyID == x.CompanyId)).All(x => x.Periods.All(p => p.IsProcessed)))
                {
                    var updateRun = _context.Runs.FirstOrDefault(x => x.RunTimestamp == _runTimestamp);
                    updateRun.WasSuccessful = true;
                    updateRun.IsCompleted = true;
                    updateRun.InProgress = false;
                    await _context.SaveChangesAsync();
                }

                await _serviceBusService.DisposeServiceBus();
            }
            catch (Exception ex)
            {
                // log error
                await _errorLogger.LogError(ex, OriginSource.ShopifyAPI, _runTimestamp, $"Error processing Shopify data.");

                // record error in run
                var updateRun = _context.Runs.FirstOrDefault(x => x.RunTimestamp == _runTimestamp);
                updateRun.WasSuccessful = false;
                updateRun.IsCompleted = true;
                updateRun.InProgress = false;
                await _context.SaveChangesAsync();

                await _serviceBusService.DisposeServiceBus();
            }
        }

        // process individual period
        // used by a single AF instance to process the target period
        public async Task ProcessPeriod(ProcessPeriodDTO periodInfo)
        {
            try
            {
                // get run info
                _runTimestamp = periodInfo.RunTimestamp;
                var _currentRun = _context.Runs.FirstOrDefault(x => x.RunTimestamp == periodInfo.RunTimestamp);

                string tempCSVLoc = Path.GetTempPath();

                // prepare CSV path
                string tempCSVFileName = periodInfo.RunTimestamp + "-" + periodInfo.CompanyId + "-" + periodInfo.PeriodId + ".csv";
                string tempCSVFilePath = Path.Combine(tempCSVLoc, tempCSVFileName);

                var dbPeriod = _context.RunCompanyPeriods.FirstOrDefault(x => x.RunTimestamp == periodInfo.RunTimestamp && x.PeriodID == periodInfo.PeriodId);
                if((periodInfo.StartAtDataID == null || periodInfo.StartAtDataID == dbPeriod.LastDataID) && (dbPeriod.ProcessingStartedAt != null || dbPeriod.IsPeriodProcessed == true))
                {
                    // period processing has already started, service bus may have sent this over again? break the process.
                    return;
                }

                // update processing start time and last data ID
                dbPeriod.LastDataID = periodInfo.StartAtDataID;
                dbPeriod.ProcessingStartedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();

                // run all available period processors
                var periodProcessors = PeriodProcessorFactory.GetPeriodProcessors(_context, _errorLogger, _serviceBusService, _shopifyService, _codatService, _stripeService);
                bool periodHasData = false;

                foreach (var processor in periodProcessors)
                {
                    // only process if processing just starts or if it should continue from the current source
                    if (periodInfo.ContinueFromSource == null || periodInfo.ContinueFromSource == processor.OriginSource)
                    {
                        var outcome = await processor.ProcessPeriod(periodInfo, tempCSVFilePath);

                        if (outcome == ProcessingOutcome.ProcessedWithData)
                            periodHasData = true;
                        else if (outcome == ProcessingOutcome.ProcessingToContinue)
                        {
                            // processing will continue from another AF instance
                            periodInfo.ContinueFromSource = processor.OriginSource;
                            return;
                        }
                    }
                }

                if (_currentRun.RunCSVs == null)
                    _currentRun.RunCSVs = new List<RunCSVEntity>();

                // save CSV information to DB
                if (!_currentRun.RunCSVs.Any(x => x.RunTimestamp == periodInfo.RunTimestamp && x.TaxuallyCompanyID == periodInfo.CompanyId && x.PeriodID == periodInfo.PeriodId))
                {
                    _currentRun.RunCSVs.Add(new RunCSVEntity
                    {
                        RunTimestamp = periodInfo.RunTimestamp,
                        CSVText = tempCSVFileName,
                        CompanyName = periodInfo.CompanyName,
                        TaxuallyCompanyID = periodInfo.CompanyId,
                        PeriodID = periodInfo.PeriodId,
                        PeriodRange = periodInfo.StartDate.ToString("yyyy-MM-dd") + "-" + periodInfo.EndDate.ToString("yyyy-MM-dd"),
                    });
                }

                await _context.SaveChangesAsync();

                // send data to Taxually
                if (periodHasData)
                {
                    var isAuthenticated = await _taxService.Authenticate(periodInfo.TaxuallyUser, periodInfo.TaxuallyPassword);
                    if (!isAuthenticated)
                    {
                        await _errorLogger.LogError(null, OriginSource.TaxuallyAPI, periodInfo.RunTimestamp, $"Failed to authenticate for company ({periodInfo.CompanyId}) {periodInfo.CompanyName}.");
                        return;
                    }

                    // upload to Taxually
                    var changeCompany = await _taxService.AuthenticateForCompany(periodInfo.CompanyId);

                    if (!changeCompany)
                    {
                        await _errorLogger.LogError(null, OriginSource.TaxuallyAPI, periodInfo.RunTimestamp, $"Failed to authenticate for company ({periodInfo.CompanyId}) {periodInfo.CompanyName}.");
                        return;
                    }

                    var runCSVInfo = _currentRun.RunCSVs.FirstOrDefault(x => x.PeriodID == periodInfo.PeriodId && x.TaxuallyCompanyID == periodInfo.CompanyId && x.RunTimestamp == periodInfo.RunTimestamp);
                    var uploadSuccess = await UploadCSVForPeriod(periodInfo, runCSVInfo, tempCSVFilePath);
                }

                dbPeriod.IsPeriodProcessed = true;

                await _context.SaveChangesAsync();

                // all periods are processed, finish run
                if (_context.RunCompanyPeriods.Where(x => x.RunTimestamp == periodInfo.RunTimestamp).All(x => x.IsPeriodProcessed != null))
                {
                    var updateRun = _context.Runs.FirstOrDefault(x => x.RunTimestamp == periodInfo.RunTimestamp);
                    updateRun.IsCompleted = true;
                    updateRun.WasSuccessful = true;
                    updateRun.InProgress = false;

                    await _context.SaveChangesAsync();
                }
            }
            catch (Exception ex)
            {
                // try updating period info
                try
                {
                    var dbPeriod = _context.RunCompanyPeriods.FirstOrDefault(x => x.RunTimestamp == periodInfo.RunTimestamp && x.PeriodID == periodInfo.PeriodId);
                    dbPeriod.IsPeriodProcessed = false;
                    await _context.SaveChangesAsync();
                }
                catch { }

                // log error
                await _errorLogger.LogError(ex, OriginSource.TaxuallyAPI, periodInfo.RunTimestamp, $"Error processing period ({periodInfo.CompanyId}) {periodInfo.CompanyName} {periodInfo.StartDate.ToString("yyyy-MM-dd")}.");
                return;
            }
        }

        // upload CSV to Taxually
        private async Task<bool> UploadCSVForPeriod(ProcessPeriodDTO periodInfo, RunCSVEntity? csvLog, string csvPath)
        {
            // open and upload file
            var csvFileStream = new FileStream(csvPath, FileMode.Open);
            var uploadStatus = await _taxService.UploadTransactionFile(new TaxuallyPeriodBatchRequest
            {
                DataPeriodID = periodInfo.PeriodId,
                UploadBatchID = periodInfo.CompanyBatchId.ToString()
            }, periodInfo, periodInfo.CompanyName, csvFileStream);

            if (uploadStatus.Error)
            {
                // error uploading, record
                await _errorLogger.LogError(new Exception(uploadStatus.Message), OriginSource.TaxuallyAPI, _runTimestamp, $"Failed to upload CSV for company ({periodInfo.CompanyId}) {periodInfo.CompanyName} and period {periodInfo.StartDate.ToString("yyyy-MM-dd")}.");
                return false;
            }
            else
            {
                if (csvLog != null)
                {
                    // record upload was a success
                    csvLog.IsUploadedToTaxually = true;
                    await _context.SaveChangesAsync();
                }

                // enqueue uploaded files on Taxually
                var enqueueStatus = await _taxService.EnqueueUploadedFiles(new TaxuallyPeriodBatchRequest
                {
                    DataPeriodID = periodInfo.PeriodId,
                    UploadBatchID = periodInfo.CompanyBatchId.ToString()
                });

                if (enqueueStatus.Error)
                {
                    // record error in the above
                    await _errorLogger.LogError(new Exception(enqueueStatus.Message), OriginSource.TaxuallyAPI, _runTimestamp, $"Failed to enqueue uploaded files for company ({periodInfo.CompanyId}) {periodInfo.CompanyName} and period {periodInfo.StartDate.ToString("yyyy-MM-dd")}.");
                    return false;
                }
                else
                {
                    if (csvLog != null)
                    {
                        // record enqueued successfully
                        csvLog.IsEnqueuedOnTaxually = true;
                        await _context.SaveChangesAsync();
                    }

                    return true;
                }
            }
        }
    }
}
