using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodatExtractor.DAL.Entities.Models
{
    public class ScheduledRunEntity
    {
        public int ID { get; set; }
        public string RunIntervalType { get; set; }
        public string RunTime { get; set; }
    }
}
