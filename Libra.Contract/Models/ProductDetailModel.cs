using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Libra.Contract.Models
{
    public class ProductDetailModel
    {
        public ProductDetailModel()
        {
            List<int> ProductIds = new List<int>();
        }

        public int Id { get; set; }

        public int GroupId { get; set; }

        public ICollection<int> ProductIds { get; set; }
    }
}
