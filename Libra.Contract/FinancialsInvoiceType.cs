using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Libra.Contract
{

    public enum FinancialsInvoiceType
    {
        /// <summary>
        /// Default invoice where total_amount > 0. Value is 1.
        /// </summary>
        Invoice = 1,

        /// <summary>
        /// Invoice to client. Value is 2.
        /// </summary>
        CreditInvoice = 2,

        /// <summary>
        /// Unified invoice. Value is 3.
        /// </summary>
        UnifiedInvoice = 3
    }

}
