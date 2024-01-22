using System;
using System.Collections.Generic;

namespace Libra.Contract
{
    public class PayoutModel
    {
        public int Id { get; set; }

        public string ActId { get; set; }

        public PayoutType Type { get; set; }

        public string Status { get; set; }

        public string TypeText { get; set; }

        public decimal Amount { get; set; }

        public int? PayerId { get; set; }

        public string Creator { get; set; }

        public string Payer { get; set; }

        public string Broker { get; set; }


        public DateTime? PayDate { get; set; }

        public ICollection<InvoiceModel> Invoices { get; set; } = new List<InvoiceModel>();
    }

    public enum PayoutType
    {
        Bank = 1,
        Custom = 3
    }
}
