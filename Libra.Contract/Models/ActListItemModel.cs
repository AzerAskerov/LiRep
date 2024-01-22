using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Libra.Contract
{
    public class ActListItemModel
    {
        public string Id { get; set; }
        public int Status { get; set; }
        public string StatusText { get; set; }
        public DateTime CreateDate { get; set; }
        public string PayoutInfo { get; set; }
        public ICollection<PayoutModel> Payouts { get; set; } = new Collection<PayoutModel>();
        public decimal Amount { get; set; }
        public string Creator { get; set; }
        public DateTime? ApprovalDate { get; set; }
        public bool? IsApproved { get; set; }
        public string ApproverFullname { get; set; }
    }
}
