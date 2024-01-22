using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Libra.Contract.Models
{
   public class QueryModel
    {
        public string ActionName { get; set; }

        public string Parameters { get; set; }

        public string DataType { get; set; }
    }

    [JsonObject]
    public class QueryResult
    {
        public bool Success { get; set; }

        public string Message { get; set; }

        
        public string Data { get; set; }

    }

    [JsonObject]
    public class Items
    {
        public List<PolicyInvoiceModel> items { get; set; }

    }

  

}
