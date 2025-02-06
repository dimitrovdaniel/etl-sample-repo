using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodatExtractor.DAL.Entities.Models
{
    public class ShopifyShopEntity
    {
        public string ShopID { get; set; }
        public string DataAccessToken { get; set; }
        public DateTime DateCreated { get; set; }

        public virtual List<ShopifyMappingEntity> ShopifyMappings { get; set; }
    }
}
