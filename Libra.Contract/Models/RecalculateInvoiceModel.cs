using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Libra.Contract
{
    public class RecalculateInvoiceModel
    {
        public string InvoiceNumber { get; set; }
        public string Status { get; set; }
        public string Error { get; set; }

    }

    public class RecalculateInvoiceListModel
    {
        public string ModuleId { get; set; }
    }
}
