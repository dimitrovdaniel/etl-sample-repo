using CodatExtractor.DAL.Entities;
using CodatExtractor.DAL.Entities.Models;
using CodatExtractor.DAL.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodatExtractor.DAL.Services
{
    // logs errors during Runs or other functionality
    public class ErrorLoggingService
    {
        private string _connectionString;

        public ErrorLoggingService(string connectionString)
        {   
            _connectionString = connectionString;
        }

        public async Task LogError(Exception ex, OriginSource source, string runTimestamp, string customMessage = null)
        {
            // log errors to DB through a separate connection to avoid failed transactions
            // record OriginSource so it's easier to debug issues
            using (COEXTRContext db = new COEXTRContext(_connectionString))
            {
                db.ErrorLogs.Add(new ErrorLogEntity
                {
                    DateCreated = DateTime.UtcNow,
                    ErrorMessage = (customMessage ?? ex?.Message) ?? "No error message.",
                    OriginSource = source.ToString(),
                    RawException = ex?.ToString() ?? "No exception.",
                    RunTimestamp = runTimestamp
                });

                await db.SaveChangesAsync();
            }
        }
    }
}
