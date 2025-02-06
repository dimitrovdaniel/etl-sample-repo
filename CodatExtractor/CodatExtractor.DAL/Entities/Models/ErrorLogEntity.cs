using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodatExtractor.DAL.Entities.Models
{
    public class ErrorLogEntity
    {
        public int ID { get; set; }
        public string RunTimestamp { get; set; }
        public string ErrorMessage { get; set; }
        public string RawException { get; set; }
        public DateTime DateCreated { get; set; }
        public string OriginSource { get; set; }
    }
}
