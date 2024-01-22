using System;
using System.Collections.Generic;

namespace Libra.Contract
{
    public class ActModel
    {
        public ActModel()
        {
            Invoices = new List<InvoiceModel>();
            Approvals = new List<ApprovalModel>();
            InsurerID = Insurer.ATESHGAH;
        }

        public string Id { get; set; }
        public ActStatus Status { get; set; }
        public ActType Type { get; set; }
        public string StatusText { get; set; }
        public string Creator { get; set; }
        public int CreatorId { get; set; }
        public string Receiver { get; set; }
        public int? ReceiverId { get; set; }
        public DateTime CreateDate { get; set; }
        public decimal Commission { get; set; }
        public decimal TotalPremium { get; set; }
        public int? InvoiceCount { get; set; }
        public ICollection<InvoiceModel> Invoices { get; }
        public ICollection<ApprovalModel> Approvals { get; }
        public bool AutoApprove { get; set; }
        public bool CustomCommissions { get; set; }
        public byte[] PrintFileContent { get; set; }
        public string PrintFileType { get; set; }
        public Insurer? InsurerID { get; set; }
        public int? BrokerID { get; set; }
        public List<UserModel> Brokers { get; set; }
        public int? GroupId { get; set; }
        public string RejectNote { get; set; }

        public string Invoice_number { get; set; }

        public PayoutType PayoutType { get; set; }

        public int Com { get; set; }

        public bool IsManual { get; set; }
    }

    public enum Insurer
    {
        ATESHGAH = 0,
        ATESHGAHLIFE = 1        
    }

    public enum ActStatus
    {
        Draft = 0,
        Sent = 1,
        Cancelled = 2,
        Rejected = 3,
        Approved = 4,
        Paid = 5
    }

    [Flags]
    public enum ActType
    {
        Primary = 1,
        Secondary = 2,
        UserComission = 4
    }
}
