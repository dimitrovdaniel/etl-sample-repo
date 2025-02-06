using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodatExtractor.DAL.Models
{
    public class ScheduledRunInfo
    {
        public int ID { get; set; }
        public string IntervalType { get; set; }
        public string RunTime { get; set; }
    }
}
