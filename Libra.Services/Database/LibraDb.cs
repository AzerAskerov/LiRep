using System.Configuration;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;

namespace Libra.Services.Database
{
    internal class LibraDb : DbContext
    {
        public LibraDb()
            : base(ConfigurationManager.ConnectionStrings["libra-db"].ConnectionString)
        {
            var instanceExists = System.Data.Entity.SqlServer.SqlProviderServices.Instance != null;
            System.Data.Entity.Database.SetInitializer<LibraDb>(null);
            // Get the ObjectContext related to this DbContext
            var objectContext = (this as IObjectContextAdapter).ObjectContext;

            // Sets the command timeout for all the commands
            objectContext.CommandTimeout = 240;
        }

        public virtual DbSet<DbUser> Users { get; set; }
        public virtual DbSet<DbInvoice> Invoices { get; set; }
        public virtual DbSet<DbPayment> Payments { get; set; }
        public virtual DbSet<DbCommission> Commissions { get; set; }
        public virtual DbSet<DbAct> Acts { get; set; }
        public virtual DbSet<DbApproval> Approvals { get; set; }
        public virtual DbSet<DbPayout> Payouts { get; set; }
        public virtual DbSet<DbActCommission> ActCommissions { get; set; }
        public virtual DbSet<DbCommissionConfig> CommissionConfigs { get; set; }
        public virtual DbSet<DbInvoiceView> InvoiceView { get; set; }
        public virtual DbSet<DbCommissionView> CommissionView { get; set; }
        public virtual DbSet<DbActInvoiceView> ActInvoiceView { get; set; }
        public virtual DbSet<DbProductDetail> ProductDetails { get; set; }
        public virtual DbSet<DbLob> Lobs { get; set; }
        public virtual DbSet<DbActView> ActView { get; set; }
        public virtual DbSet<DbActApprovalView> ActApprovalView { get; set; }
        public virtual DbSet<CommonWebApiLog> CommonWebApiLog { get; set; }
        public virtual DbSet<RecalculatedInvoiceLog> RecalculatedInvoiceLogs { get; set; }

        public virtual DbSet<DbManualCommission> ManualCommissions { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Entity<DbAct>()
                .HasMany(e => e.Approvals)
                .WithRequired()
                .HasForeignKey(e => e.ActId)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<DbAct>()
                .HasMany(e => e.Payouts)
                .WithRequired(e => e.Act)
                .HasForeignKey(e => e.ActId)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<DbAct>()
                .HasMany(e => e.Commissions)
                .WithRequired()
                .HasForeignKey(e => e.ActId)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<DbAct>()
                .HasMany(e => e.Invoices)
                .WithRequired()
                .HasForeignKey(e => e.ActId)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<DbInvoice>()
                .HasMany(e => e.Payments)
                .WithRequired(e => e.Invoice)
                .HasForeignKey(e => e.InvoiceId)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<DbInvoice>()
                .HasMany(e => e.Commissions)
                .WithRequired(e => e.Invoice)
                .HasForeignKey(e => e.InvoiceId)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<DbUser>()
                .HasMany(e => e.Approvals)
                .WithOptional(e => e.Approver)
                .HasForeignKey(e => e.ApproverId)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<DbUser>()
                .HasMany(e => e.Payouts)
                .WithOptional(e => e.Payer)
                .HasForeignKey(e => e.PayerId)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<DbUser>()
                .HasMany(e => e.ActsCreated)
                .WithRequired(e => e.Creator)
                .HasForeignKey(e => e.CreatorId)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<DbUser>()
                .HasMany(e => e.ActsReceived)
                .WithRequired(e => e.Receiver)
                .HasForeignKey(e => e.ReceiverId)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<DbUser>()
                .HasMany(e => e.Invoices)
                .WithRequired(e => e.Creator)
                .HasForeignKey(e => e.CreatorId)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<DbInvoiceView>()
                .HasMany(e => e.Commissions)
                .WithRequired()
                .HasForeignKey(e => e.InvoiceId)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<DbInvoiceView>()
                .Property(e => e.Id)
                .IsUnicode(false);

            modelBuilder.Entity<DbInvoiceView>()
                .Property(e => e.PolicyNumber)
                .IsUnicode(false);

            modelBuilder.Entity<DbInvoiceView>()
                .Property(e => e.BeneficiaryCode)
                .IsUnicode(false);

            modelBuilder.Entity<DbActView>()
                .HasMany(e => e.Payouts)
                .WithOptional()
                .HasForeignKey(e => e.ActId)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<DbActInvoiceView>()
                .HasMany(e => e.Commissions)
                .WithRequired()
                .HasForeignKey(e => new {e.InvoiceId, e.ActId})
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<DbCommissionView>()
                .Property(e => e.InvoiceId)
                .IsUnicode(false);

            modelBuilder.Entity<DbPayout>()
                .HasMany(e => e.Invoices)
                .WithRequired(e => e.Payout)
                .HasForeignKey(e => e.Id)
                .WillCascadeOnDelete(false);

            //modelBuilder.Entity<DbProductDetail>()
            //    .HasMany(e => e.UserGroup);

            modelBuilder.Entity<CommonWebApiLog>()
                .Property(e => e.Path)
                .IsUnicode(false);

            modelBuilder.Entity<CommonWebApiLog>()
                .Property(e => e.SourceIp)
                .IsUnicode(false);

            modelBuilder.Entity<RecalculatedInvoiceLog>()
                .Property(e => e.InvoiceId)
                .IsUnicode(false);

            modelBuilder.Entity<RecalculatedInvoiceLog>()
                .Property(e => e.Status)
                .IsUnicode(false);

            modelBuilder.Entity<DbManualCommission>()
                .HasIndex(e => e.Id);

            modelBuilder.Entity<DbManualCommission>()
                .Property(e => e.InvoiceId)
                .HasMaxLength(50)
                .IsRequired()
                ;

            modelBuilder.Entity<DbManualCommission>()
                .Property(e => e.Amount)
                .HasPrecision(9, 2)
                .IsRequired();

            modelBuilder.Entity<DbManualCommission>()
                .Property(e => e.ActType)
                .IsRequired();

            modelBuilder.Entity<DbManualCommission>()
                .Property(e => e.PayoutType)
                .IsRequired();
        }
    }
}