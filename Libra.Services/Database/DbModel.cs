using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Libra.Services.Database
{
    [Table("LOB", Schema = "dbo")]
    internal class DbLob
    {
        public int Id { get; set; }
        public int lob_oid { get; set; }
        public bool approvable { get; set; }
    }

    [Table("ProductDetail", Schema = "dbo")]
    internal class DbProductDetail
    {
        public int Id { get; set; }
        public int GroupId { get; set; }
        public string ProductIds { get; set; }
    }

    [Table("User", Schema = "dbo")]
    internal class DbUser
    {
        public int Id { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public string Name { get; set; }
        public string Surname { get; set; }
        public int Role { get; set; }
        public string Supervisors { get; set; }
        public string ProductGroup { get; set; }
        public string Email { get; set; }
        public ICollection<DbAct> ActsCreated { get; } = new List<DbAct>();
        public ICollection<DbAct> ActsReceived { get; } = new List<DbAct>();
        public ICollection<DbInvoice> Invoices { get; } = new List<DbInvoice>();
        public ICollection<DbApproval> Approvals { get; } = new List<DbApproval>();
        public ICollection<DbPayout> Payouts { get; } = new List<DbPayout>();
    }

    [Table("Invoice", Schema = "dbo")]
    internal class DbInvoice
    {
        public string Id { get; set; }
        public string PolicyNumber { get; set; }
        public int Product { get; set; }
       
        public int PolicyHolderType { get; set; }
        public string PolicyHolderCode { get; set; }
        public string PolicyHolderName { get; set; }
        public int BeneficiaryType { get; set; }
        public string BeneficiaryCode { get; set; }
        public string BeneficiaryName { get; set; }
        public decimal Premium { get; set; }
        public decimal PayablePremium { get; set; }
        public DateTime CreateDate { get; set; }
        public int CreatorId { get; set; }
        public DbUser Creator { get; set; }
        public int ProcessStatus { get; set; }
        public bool? IsCommissionFromIms  { get; set; }
        public ICollection<DbPayment> Payments { get; } = new List<DbPayment>();
        public ICollection<DbCommission> Commissions { get; } = new List<DbCommission>();

        #region ignored but used for calculation commission
        [NotMapped]
        public int? VehicleType { get; set; }
        [NotMapped]
        public decimal? EngineCapacity { get; set; }
        [NotMapped]
        public string LoginRegion { get; set; }
        [NotMapped]
        //insuredAddressregion prop
        public string InsuredAddressRegion { get; set; }
        //vehicle region
        [NotMapped]
        public string VehicleRegion { get; set; }
        //vehicle model
        [NotMapped]
        public string VehicleModel { get; set; }
        //vehicle brand
        [NotMapped]
        public string VehicleBrand { get; set; }
        //vehicle age
        [NotMapped]
        public int? VehicleAge { get; set; }

   


        #endregion
    }

    [Table("Payment", Schema = "dbo")]
    internal class DbPayment
    {
        public int Id { get; set; }
        public string InvoiceId { get; set; }
        public decimal Amount { get; set; }
        public DateTime PayDate { get; set; }
        public DbInvoice Invoice { get; set; }
    }

    [Table("Commission", Schema = "dbo")]
    internal class DbCommission
    {
        [Column(Order = 0), Key]
        public string InvoiceId { get; set; }
        [Column(Order = 1), Key]
        public int PayoutType { get; set; }
        [Column(Order = 2), Key]
        public int ActType { get; set; }
        public decimal Amount { get; set; }
        public DbInvoice Invoice { get; set; }
    }

    [Table("Act", Schema = "dbo")]
    internal class DbAct
    {
        public string Id { get; set; }
        public int Status { get; set; }
        public int Type { get; set; }
        public int CreatorId { get; set; }
        public int ReceiverId { get; set; }
        public DateTime CreateDate { get; set; }
        public int? PayerId { get; set; }
        public DbUser Creator { get; set; }
        public DbUser Receiver { get; set; }
        public ICollection<DbApproval> Approvals { get; } = new List<DbApproval>();
        public ICollection<DbPayout> Payouts { get; } = new List<DbPayout>();
        public ICollection<DbActCommission> Commissions { get; } = new List<DbActCommission>();
        public ICollection<DbActInvoiceView> Invoices { get; } = new List<DbActInvoiceView>();
        public int? Insurer { get; set; }
        public DbUser Broker { get; set; }
        public int? BrokerId { get; set; }

        public int? GroupId { get; set; }
    }

    [Table("ActCommission", Schema = "dbo")]
    internal class DbActCommission
    {
        public int Id { get; set; }
        public string ActId { get; set; }
        public string InvoiceId { get; set; }
        public int PayoutType { get; set; }
        public decimal Amount { get; set; }
    }

    [Table("Approval", Schema = "dbo")]
    internal class DbApproval
    {
        public int Id { get; set; }
        public string ActId { get; set; }
        public int? ApproverId { get; set; }
        public DateTime? ApproveDate { get; set; }
        public DateTime? RejectDate { get; set; }
        public string RejectNote { get; set; }
        public DbUser Approver { get; set; }
    }

    [Table("Payout", Schema = "dbo")]
    internal class DbPayout
    {
        public int Id { get; set; }
        public string ActId { get; set; }
        public int Type { get; set; }
        public decimal Amount { get; set; }
        public int? PayerId { get; set; }
        public DateTime? PayDate { get; set; }
        public DbAct Act { get; set; }
        public DbUser Payer { get; set; }
        public ICollection<DbPayoutInvoiceView> Invoices { get; } = new List<DbPayoutInvoiceView>();
    }

    [Table("CommissionConfig", Schema = "dbo")]
    internal class DbCommissionConfig
    {
        public int Id { get; set; }
        public int ActType { get; set; }
        public int PayoutType { get; set; }
        public int? Product { get; set; }
        public int? Brand { get; set; }
        public int? PolicyHolderType { get; set; }
        public string Username { get; set; }
        public string BeneficiaryCode { get; set; }
        public decimal AmountFixed { get; set; }
        public decimal AmountPercent { get; set; }
        public decimal? AmountMin { get; set; }
        public decimal? AmountMax { get; set; }
        public DateTime ValidFrom { get; set; }
        public DateTime ValidTo { get; set; }
        public decimal PremiumFrom { get; set; }
        public decimal PremiumTo { get; set; }
        public int? VehicleType { get; set; }
        public decimal? EngineCapacityFrom { get; set; }
        public decimal? EngineCapacityTo { get; set; }

        //brand name
        public string BrandName { get; set; }
        //model name

        public string ModelName { get; set; }
        //vehicle age from
        public int? VehicleAgeFrom { get; set; }
        //vehicle age to
        public int? VehicleAgeTo { get; set; }
        //vehicle region
        public string VehicleRegion { get; set; }
        //insuredAddressregion prop
        public string InsuredAddressRegion { get; set; }
        //login region
        public string LoginRegion { get; set; }

    }

    [Table("RecalculatedInvoiceLog", Schema = "dbo")]
    internal class RecalculatedInvoiceLog
    {
        public int Id { get; set; }

        public string InvoiceId { get; set; }

        public string Status { get; set; }

        public string ErrorDescription { get; set; }
    }

    internal interface IInvoiceView
    {
        string Id { get; }
        string PolicyNumber { get; }
        string PolicyHolder { get; }
        int Product { get; }
        string Brand { get; }
        string Beneficiary { get; }
        decimal Premium { get; }
        decimal PayablePremium { get; }
        DateTime CreateDate { get; }
        int CreatorId { get; }
        string Creator { get; set; }
        int ProcessStatus { get; }
        decimal PaidPremium { get; }
        decimal UnpaidPremium { get; }
        decimal WithheldCommission { get; }
        bool? IsCommissionFromIms { get; }
    }

    [Table("InvoiceView", Schema = "dbo")]
    internal class DbInvoiceView : IInvoiceView
    {
        public string Id { get; set; }
        public string PolicyNumber { get; set; }
        public string PolicyHolder { get; set; }
        public int Product { get; set; }
        public string Brand { get; set; }
        public string Beneficiary { get; set; }
        public string BeneficiaryCode { get; set; }
        public decimal Premium { get; set; }
        public decimal PayablePremium { get; set; }
        public DateTime CreateDate { get; set; }
        public DateTime PayDate { get; set; }
        public int CreatorId { get; set; }
        public string Creator { get; set; }
        public string CreatorCode { get; set; }
        public int ProcessStatus { get; set; }
        public decimal PaidPremium { get; set; }
        public decimal UnpaidPremium { get; set; }
        public decimal WithheldCommission { get; set; }
        public bool? IsCommissionFromIms { get; set; }
        public ICollection<DbCommissionView> Commissions { get; set; } = new List<DbCommissionView>();
    }

    [Table("ActInvoiceView", Schema = "dbo")]
    internal class DbActInvoiceView : IInvoiceView
    {
        [Column(Order = 0), Key]
        public string Id { get; set; }
        [Column(Order = 1), Key]
        public string ActId { get; set; }
        public string PolicyNumber { get; set; }
        public string PolicyHolder { get; set; }
        public int Product { get; set; }
        public string Brand { get; set; }
        public string Beneficiary { get; set; }
        public decimal Premium { get; set; }
        public decimal PayablePremium { get; set; }
        public DateTime CreateDate { get; set; }
        public int CreatorId { get; set; }
        public string Creator { get; set; }
        public int ProcessStatus { get; set; }
        public decimal PaidPremium { get; set; }
        public decimal UnpaidPremium { get; set; }
        public decimal WithheldCommission { get; set; }
        public bool? IsCommissionFromIms { get; set; }
        public ICollection<DbActCommissionView> Commissions { get; } = new List<DbActCommissionView>();
    }

    internal interface ICommissionView
    {
        string InvoiceId { get; }
        int PayoutType { get; }
        int ActType { get; }
        decimal TotalAmount { get; }
        decimal TotalPercent { get; }
        decimal PaidAmount { get; }
        decimal PaidPercent { get; }
        decimal UnpaidAmount { get; }
        decimal UnpaidPercent { get; }
        decimal CustomAmount { get; }
        decimal CustomPercent { get; }
        bool IsManual { get; }
    }

    [Table("CommissionView", Schema = "dbo")]
    internal class DbCommissionView : ICommissionView
    {
        [Column(Order = 0), Key]
        public string InvoiceId { get; set; }
        [Column(Order = 1), Key]
        public int PayoutType { get; set; }
        [Column(Order = 2), Key]
        public int ActType { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal TotalPercent { get; set; }
        public decimal PaidAmount { get; set; }
        public decimal PaidPercent { get; set; }
        public decimal UnpaidAmount { get; set; }
        public decimal UnpaidPercent { get; set; }
        public decimal CustomAmount { get; set; }
        public decimal CustomPercent { get; set; }
        public bool IsManual { get; set; }
    }

    [Table("ActCommissionView", Schema = "dbo")]
    internal class DbActCommissionView : ICommissionView
    {
        [Column(Order = 0), Key]
        public string InvoiceId { get; set; }
        [Column(Order = 1), Key]
        public string ActId { get; set; }
        [Column(Order = 2), Key]
        public int PayoutType { get; set; }
        public int ActType { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal TotalPercent { get; set; }
        public decimal PaidAmount { get; set; }
        public decimal PaidPercent { get; set; }
        public decimal UnpaidAmount { get; set; }
        public decimal UnpaidPercent { get; set; }
        public decimal CustomAmount { get; set; }
        public decimal CustomPercent { get; set; }
        public bool IsManual { get; set; }
    }

    [Table("ActView", Schema = "dbo")]
    internal class DbActView
    {
        public string Id { get; set; }
        public int Status { get; set; }
        public int CreatorId { get; set; }
        public DateTime CreateDate { get; set; }
        public decimal Amount { get; set; }
        public string ReceiverCode { get; set; }
        public int PayoutType { get; set; }
        public ICollection<DbPayout> Payouts { get; } = new List<DbPayout>();
    }

    [Table("ActApprovalView", Schema = "dbo")]
    internal class DbActApprovalView
    {
        public string Id { get; set; }
        public int Status { get; set; }
        public int CreatorId { get; set; }
        public DateTime CreateDate { get; set; }
        public int? ApproverId { get; set; }
        public DateTime? ApproveDate { get; set; }
        public DateTime? RejectDate { get; set; }
        public string Creator { get; set; }
        public decimal Amount { get; set; }
        public int GroupId { get; set; }
        public string ApproverFullname { get; set; }
    }

    [Table("PayoutInvoiceView", Schema = "dbo")]
    internal class DbPayoutInvoiceView
    {
        [Column(Order = 0), Key]
        public int Id { get; set; }
        [Column(Order = 1), Key]
        public string InvoiceId { get; set; }
        public string PolicyNumber { get; set; }
        public string PolicyHolder { get; set; }
        public decimal Amount { get; set; }
        public DbPayout Payout { get; set; }
    }




    [Table("CommonWebApiLog", Schema = "dbo")]
    public partial class CommonWebApiLog
    {
        [Key]
        public int LogOid { get; set; }

        public DateTime Timestamp { get; set; }

        [StringLength(200)]
        public string Path { get; set; }

        [StringLength(40)]
        public string SourceIp { get; set; }

        public string Request { get; set; }

        public string Response { get; set; }

        public short? ResponseCode { get; set; }

        public int? UserId { get; set; }

        [StringLength(1000)]
        public string ObjectIdentifier { get; set; }
    }

    [Table("ManualCommission", Schema = "dbo")]
    internal class DbManualCommission
    {
        public int Id { get; set; }
        public string InvoiceId { get; set; }
        public decimal Amount { get; set; }
        public int ActType { get; set; }
        public int PayoutType { get; set; }
    }
}