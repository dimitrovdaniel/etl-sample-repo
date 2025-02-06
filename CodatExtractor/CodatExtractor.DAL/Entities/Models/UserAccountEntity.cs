using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodatExtractor.DAL.Entities.Models
{
    public class UserAccountEntity
    {
        public string Username { get; set; }
        public string PasswordPlain { get; set; }
        public bool HasAdminRights { get; set; }
        public DateTime DateCreated { get; set; }
    }
}
