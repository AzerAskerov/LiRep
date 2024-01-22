using System.Collections.Generic;

namespace Libra.Contract
{
    public class InvoiceModel
    {
        public string Number { get; set; }
        public string PolicyNumber { get; set; }
        public string PolicyHolder { get; set; }
        public string Product { get; set; }
        public string Brand { get; set; }
        public string Beneficiary { get; set; }
        public decimal Premium { get; set; }
        public decimal PayablePremium { get; set; }
        public decimal PaidPremium { get; set; }
        public decimal UnpaidPremium { get; set; }
        public decimal WithheldCommission { get; set; }
        public int CreatorId { get; set; }
        public string Creator { get; set; }
        public bool IsCommissionFromIms { get; set; }
        public AmountModel Commission { get; set; }
        public ICollection<CommissionModel> Commissions { get; set; } = new List<CommissionModel>();
    }

    public class InvoiceListModel
    {
        public string ModuleId { get; set; }

        public bool ReadOnly { get; set; }
    }

    public enum PersonType
    {
        Physical = 1,
        Legal = 2
    }

    public enum InvoiceStatus
    {
        Paid = 1
        //PartiallyPaid = 2
    }

    public class InvoiceViewModel
    {
        public ICollection<InvoiceModel> InvoicesModel;
        public int ItemsCount { get; set; }
    }
}
