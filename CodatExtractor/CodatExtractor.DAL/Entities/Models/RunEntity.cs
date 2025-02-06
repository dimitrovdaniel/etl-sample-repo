using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodatExtractor.DAL.Entities.Models
{
    public class RunEntity
    {
        public string RunTimestamp { get; set; }
        public bool IsCompleted { get; set; }
        public bool InProgress { get; set; }
        public bool WasSuccessful { get; set; }
        public bool WasUserInvoked { get; set; }
        public string InvokedByUser { get; set; }
        public DateTime DateExecuted { get; set; }
        public int? RunForCompany { get; set; }
        public string RunForPeriod { get; set; }
        public virtual ICollection<RunCSVEntity> RunCSVs { get; set; }
        public virtual ICollection<RunCompanyPeriodEntity> RunCompanyPeriods { get; set; }
    }
}
