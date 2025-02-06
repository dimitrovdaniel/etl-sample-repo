using CodatExtractor.DAL.Models;

namespace CodatExtractor.DAL.Services.PeriodProcessors
{
    public interface IPeriodProcessor
    {
        // holds the source type
        OriginSource OriginSource { get; }

        // processes the period
        Task<ProcessingOutcome> ProcessPeriod(ProcessPeriodDTO periodInfo, string tempCSVFilePath);
    }
}