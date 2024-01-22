using System;
using System.Collections.Generic;
using System.Collections.Specialized;

namespace Libra.Contract
{
    public class InvoiceFilter
    {
        public List<int> UserIds { get; set; }
        public ActType Type { get; set; }
        public string Invoice { get; set; }
        public string Policy { get; set; }
        public DateTime? CreateDateFrom { get; set; }
        public DateTime? CreateDateTill { get; set; }
        public DateTime? PayDateFrom { get; set; }
        public DateTime? PayDateTill { get; set; }
        public InvoiceStatus? Status { get; set; }
        public Product? Product { get; set; }
        public string Agent { get; set; }
        public string Beneficiary { get; set; }
        public decimal Premium { get; set; }
        public InvoiceModel InvoiceModel { get; set; }
        public PayoutType? PayoutType { get; set; }
        public int PageNumber { get; set; } = 1;

        public InvoiceFilter()
        {
            UserIds = new List<int>();
        }
    }

    public class ActFilter
    {
        public bool IsUnderwriter { get; set; }
        public int? CreatorId { get; set; }
        public string Creator { get; set; }
        public int? ApproverId { get; set; }
        public int? Status { get; set; }
        public string Receiver { get; set; }
        public decimal Amount { get; set; }
        public string ActNumber { get; set; }
        public List<string> ListActNumber { get; set; }
        public string PolicyNumber { get; set; }
        public string InvoiceNumber { get; set; }
        public DateTime? CreateDateFrom { get; set; }
        public DateTime? CreateDateTill { get; set; }
        public DateTime? PayoutDateFrom { get; set; }
        public DateTime? PayoutDateTill { get; set; }
        public DateTime? ApproveDateFrom { get; set; }
        public DateTime? ApproveDateTill { get; set; }

        public List<PayoutType> Types { get; set; } = new List<PayoutType>();

        public List<int> UserIds { get; set; } = new List<int>();

        public ActFilter()
        {
            ListActNumber = new List<string>();
        }
    }

    public class ApprovalsFilter
    {
        public string ActNumber { get; set; }
        public int? Status { get; set; }
        public DateTime? CreateDateFrom { get; set; }
        public DateTime? CreateDateTill { get; set; }
        public DateTime? ApprovalDateFrom { get; set; }
        public DateTime? ApprovalDateTill { get; set; }
        public decimal Amount { get; set; }
        public string Creator { get; set; }
    }

    public class PayoutFilter
    {
        public string ActNumber { get; set; }
        public int? Status { get; set; }
        public string PolicyNumber { get; set; }
        public decimal Amount { get; set; }
        public DateTime? PayoutDateFrom { get; set; }
        public DateTime? PayoutDateTill { get; set; }
        public string Creator { get; set; }
        public int? PayerId { get; set; }
        public List<string> ListActNumber { get; set; }
        public int? Type { get; set; }
        public ICollection<PayoutType> Types { get; set; } = new List<PayoutType>();

        public PayoutFilter()
        {
            ListActNumber = new List<string>();
        }
    }
    
    public class CommissionFilter
    {
        public int? ActType { get; set; }
        public int? PayoutType { get; set; }
        public int? Product { get; set; }
        public int? Brand { get; set; }
        public int? PolicyHolderType { get; set; }
        public string Agent { get; set; }
        public string BeneficiaryCode { get; set; }
        //public decimal AmountFixed { get; set; }
        //public decimal AmountPercent { get; set; }
        //public decimal? AmountMin { get; set; }
        //public decimal? AmountMax { get; set; }
        public DateTime? ValidFrom { get; set; }
        public DateTime? ValidTo { get; set; }
        public decimal? PremiumFrom { get; set; }
        public decimal? PremiumTo { get; set; }
        public int? VehicleType { get; set; }
        public decimal? EngineCapacityFrom { get; set; }
        public decimal? EngineCapacityTo { get; set; }
    }
    
    public class UserFilter
    {
    }
}