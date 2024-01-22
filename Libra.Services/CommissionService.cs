using Libra.Contract;
using Libra.Contract.Models;
using Libra.Services.Database;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;

namespace Libra.Services
{
    public class CommissionService : ICommissionService
    {
        private readonly ITranslationProvider translationProvider;

        public CommissionService(ITranslationProvider translationProvider)
        {
            this.translationProvider = translationProvider;
        }


        public OperationResult<ICollection<CommissionConfigModel>> Load(CommissionFilter commissionFilter)
        {
            using (var db = new LibraDb())
            {
                var ttt = db.CommissionConfigs
                    .Where
                    (
                    cf =>
                     (!commissionFilter.ActType.HasValue || cf.ActType == commissionFilter.ActType) &&
                     (string.IsNullOrEmpty(commissionFilter.Agent) || cf.Username == commissionFilter.Agent) &&
                     (!commissionFilter.Product.HasValue || cf.Product == commissionFilter.Product.Value) &&
                     (!commissionFilter.Brand.HasValue || cf.Brand == commissionFilter.Brand.Value) &&
                     (!commissionFilter.PolicyHolderType.HasValue || cf.PolicyHolderType == commissionFilter.PolicyHolderType) &&
                     (!commissionFilter.PayoutType.HasValue || cf.PayoutType == commissionFilter.PayoutType) &&
                     (string.IsNullOrEmpty(commissionFilter.BeneficiaryCode) || cf.BeneficiaryCode == commissionFilter.BeneficiaryCode) &&
                     (!commissionFilter.ValidFrom.HasValue || cf.ValidFrom >= commissionFilter.ValidFrom) &&
                     (!commissionFilter.ValidTo.HasValue || cf.ValidTo <= commissionFilter.ValidTo) &&
                     (!commissionFilter.PremiumFrom.HasValue || cf.PremiumFrom >= commissionFilter.PremiumFrom) &&
                     (!commissionFilter.PremiumTo.HasValue || cf.PremiumTo <= commissionFilter.PremiumTo) &&
                     (!commissionFilter.VehicleType.HasValue || cf.VehicleType == commissionFilter.VehicleType) &&
                     (!commissionFilter.EngineCapacityFrom.HasValue || cf.EngineCapacityFrom >= commissionFilter.EngineCapacityFrom) &&
                     (!commissionFilter.EngineCapacityTo.HasValue || cf.EngineCapacityTo <= commissionFilter.EngineCapacityTo)
                    );
                var tc = ttt.Count();
                    var commissions = ttt
                    .Select(c => new CommissionConfigModel
                    {
                        Id = c.Id,
                        Username = c.Username,
                        Product = c.Product,
                        ActType = c.ActType,
                        PayoutType = c.PayoutType,
                        PolicyHolderType = c.PolicyHolderType,
                        BeneficiaryCode = c.BeneficiaryCode,
                        AmountFixed = c.AmountFixed,
                        AmountPercent = c.AmountPercent,
                        AmountMin = c.AmountMin,
                        AmountMax = c.AmountMax,
                        Brand = c.Brand,
                        ValidFrom = (DateTime)DbFunctions.TruncateTime(c.ValidFrom),
                        ValidTo = (DateTime)DbFunctions.TruncateTime(c.ValidTo),
                        PremiumFrom = c.PremiumFrom,
                        PremiumTo = c.PremiumTo,
                        VehicleType = c.VehicleType,
                        EngineCapacityFrom = c.EngineCapacityFrom,
                        EngineCapacityTo = c.EngineCapacityTo
                    })
                    .Take(200).ToList();

                if (commissions.Count == 0)
                {
                    commissions.Add(new CommissionConfigModel() { Brand = -1 });
                }

               

                return new OperationResult<ICollection<CommissionConfigModel>>(commissions);
            }
        }

        public OperationResult Save(ICollection<CommissionConfigModel> model)
        {

            model.Remove(model.FirstOrDefault(x => x.Brand == -1));

            using (var db = new LibraDb())
            {

                var dbCommissions = db.CommissionConfigs
                    .ToDictionary(c => new CommissionConfigKey(
                        c.ActType,
                        c.PayoutType,
                        c.Product,
                        c.PolicyHolderType,
                        c.Username,
                        c.BeneficiaryCode,
                        c.Brand,
                        c.ValidFrom,
                        c.ValidTo,
                        c.PremiumFrom,
                        c.PremiumTo));

                var keys = new HashSet<CommissionConfigKey>();

                foreach (var commission in model)
                {
                    var key = new CommissionConfigKey(
                        commission.ActType,
                        commission.PayoutType,
                        commission.Product,
                        commission.PolicyHolderType,
                        commission.Username?.ToUpperInvariant(),
                        commission.BeneficiaryCode?.ToUpperInvariant(),
                        commission.Brand,
                        commission.ValidFrom,
                        commission.ValidTo,
                        commission.PremiumFrom,
                        commission.PremiumTo);

                    if (keys.Contains(key))
                    {
                        return new OperationResult(translationProvider.GetString("DUPLICATE_COMMISSIONS"), IssueSeverity.Error);
                    }

                    keys.Add(key);

                    DbCommissionConfig dbCommission;
                    if (dbCommissions.TryGetValue(key, out dbCommission))
                    {
                        dbCommission.AmountFixed = commission.AmountFixed;
                        dbCommission.AmountPercent = commission.AmountPercent;
                        dbCommission.AmountMin = commission.AmountMin;
                        dbCommission.AmountMax = commission.AmountMax;

                        dbCommissions.Remove(key);
                    }
                    else
                    {
                        dbCommission = new DbCommissionConfig
                        {
                            ActType = commission.ActType,
                            PayoutType = commission.PayoutType,
                            Product = commission.Product,
                            Brand = commission.Brand,
                            PolicyHolderType = commission.PolicyHolderType,
                            Username = commission.Username?.ToUpperInvariant(),
                            BeneficiaryCode = commission.BeneficiaryCode?.ToUpperInvariant(),
                            AmountFixed = commission.AmountFixed,
                            AmountPercent = commission.AmountPercent,
                            AmountMin = commission.AmountMin,
                            AmountMax = commission.AmountMax,
                            ValidFrom = commission.ValidFrom,
                            ValidTo = commission.ValidTo,
                            PremiumFrom = commission.PremiumFrom,
                            PremiumTo = commission.PremiumTo
                        };

                        db.CommissionConfigs.Add(dbCommission);
                    }
                }

                dbCommissions
                    .Values
                    .ToList()
                    .ForEach(c => db.CommissionConfigs.Remove(c));

                db.SaveChanges();
            }

            return new OperationResult(translationProvider.GetString("COMMISSIONS_SAVED"), IssueSeverity.Success);

        }

        public OperationResult Update(CommissionConfigModel model)
        {
            var successKey = "COMMISSION_UPDATED";
            if (!model.Id.HasValue)
                successKey = "COMMISSION_INSERTED";

            var result = new OperationResult(translationProvider.GetString(successKey), IssueSeverity.Success);

            using (var db = new LibraDb())
            {
                if (model.Id.HasValue)
                {
                    var itemToUpdate = db.CommissionConfigs.FirstOrDefault(x => x.Id == model.Id);
                    itemToUpdate.ActType = model.ActType;
                    itemToUpdate.PayoutType = model.PayoutType;
                    itemToUpdate.Product = model.Product;
                    itemToUpdate.Brand = model.Brand;
                    itemToUpdate.PolicyHolderType = model.PolicyHolderType;
                    itemToUpdate.Username = model.Username?.ToUpperInvariant();
                    itemToUpdate.BeneficiaryCode = model.BeneficiaryCode?.ToUpperInvariant();
                    itemToUpdate.AmountFixed = model.AmountFixed;
                    itemToUpdate.AmountPercent = model.AmountPercent;
                    itemToUpdate.AmountMin = model.AmountMin;
                    itemToUpdate.AmountMax = model.AmountMax;
                    itemToUpdate.ValidFrom = model.ValidFrom;
                    itemToUpdate.ValidTo = model.ValidTo;
                    itemToUpdate.PremiumFrom = model.PremiumFrom;
                    itemToUpdate.PremiumTo = model.PremiumTo;
                    itemToUpdate.VehicleType = model.VehicleType;
                    itemToUpdate.EngineCapacityFrom = model.EngineCapacityFrom;
                    itemToUpdate.EngineCapacityTo = model.EngineCapacityTo;
                }
                else
                {
                    var dbCommission = new DbCommissionConfig
                    {
                        ActType = model.ActType,
                        PayoutType = model.PayoutType,
                        Product = model.Product,
                        Brand = model.Brand,
                        PolicyHolderType = model.PolicyHolderType,
                        Username = model.Username?.ToUpperInvariant(),
                        BeneficiaryCode = model.BeneficiaryCode?.ToUpperInvariant(),
                        AmountFixed = model.AmountFixed,
                        AmountPercent = model.AmountPercent,
                        AmountMin = model.AmountMin,
                        AmountMax = model.AmountMax,
                        ValidFrom = model.ValidFrom,
                        ValidTo = model.ValidTo,
                        PremiumFrom = model.PremiumFrom,
                        PremiumTo = model.PremiumTo,
                        VehicleType = model.VehicleType,
                        EngineCapacityFrom = model.EngineCapacityFrom,
                        EngineCapacityTo = model.EngineCapacityTo
                    };

                    db.CommissionConfigs.Add(dbCommission);
                }

                db.SaveChanges();
            }

            return result;
        }

        public OperationResult Delete(CommissionConfigModel model)
        {
            var result = new OperationResult(translationProvider.GetString("COMMISSION_DELETED"), IssueSeverity.Success);

            if (!model.Id.HasValue)
                return result;

            using (var db = new LibraDb())
            {
                var itemToDelete = db.CommissionConfigs.FirstOrDefault(x => x.Id == model.Id);
                db.CommissionConfigs.Remove(itemToDelete);
                db.SaveChanges();
            }

            return result;
        }

        public OperationResult SaveComissionChanges(GeneralResultModel model)
        {
            //variables
            var result = new OperationResult();

            IEnumerable<DbManualCommission> dbComissions = model.Changes

              .Select(m => new DbManualCommission
              {
                  InvoiceId = m.Invoice_number,
                  Amount = m.Amount,
                  ActType = (int)m.Acttype,
                  PayoutType = (int)m.Payouttype
              });
            
            #region validation

            var res = model.ValidateComissionChanges(model.Changes);

            if (res.IsSuccess == false)
            {
                result.Issues.Add(new Issue(translationProvider.GetString(res.Message), IssueSeverity.Error));

            }

            foreach (var item in dbComissions)
            {
                bool ifexist = ((OperationResult<string>)CheckInvoice(item.InvoiceId)).Model != null;

                if (ifexist)
                    result.Issues.Add(new Issue($"Invoice {item.InvoiceId} already exist in System", IssueSeverity.Error));
            }

            #endregion

            if (result.IsSuccess)
            {
                using (var db = new LibraDb())
                {
                    foreach (var item in dbComissions)
                    {
                        var commissions = db.ManualCommissions.Where(m => m.InvoiceId == item.InvoiceId).ToList();

                        var actTypeExist = commissions.Exists(c => c.ActType == item.ActType);

                        if (commissions.Count == 0 || (commissions.Count > 0 && !actTypeExist))
                        {
                            db.ManualCommissions.Add(item);
                        }
                        else
                        {
                            var commission  =  commissions.FirstOrDefault(c => c.InvoiceId == item.InvoiceId && c.ActType == item.ActType);

                            commission.PayoutType = item.PayoutType;
                            commission.Amount = item.Amount;
                        }
                    }
                    db.SaveChanges();
                }

                return new OperationResult(translationProvider.GetString("MANUAL_COMMISSION_ADDED"), IssueSeverity.Success);
            }

            return result;
        }

        public OperationResult CheckInvoice(string invoice_number)
        {
            using (var db = new LibraDb())
            {
                var invoice = db.Invoices.FirstOrDefault(i => i.Id == invoice_number);
                if (invoice == null)
                {
                    return new OperationResult<string>
                    {
                        Model = null
                    };
                }
            }

            return new OperationResult<string>
            {
                Model = invoice_number

            };
        }
    }

    internal struct CommissionConfigKey : IEquatable<CommissionConfigKey>
    {
        private readonly int actType;
        private readonly int payoutType;
        private readonly int? product;
        private readonly int? policyHolderType;
        private readonly string username;
        private readonly string beneficiaryCode;
        private readonly int? brand;
        private readonly DateTime? ValidFrom;
        private readonly DateTime? ValidTo;
        private readonly decimal? premiumFrom;
        private readonly decimal? premiumTo;

        public CommissionConfigKey(int actType, int payoutType, int? product, int? policyHolderType, string username, string beneficiaryCode, int? brand, DateTime? validFrom, DateTime? validTo, decimal? premiumFrom, decimal? premiumTo)
        {
            this.username = username;
            this.product = product;
            this.actType = actType;
            this.payoutType = payoutType;
            this.policyHolderType = policyHolderType;
            this.beneficiaryCode = beneficiaryCode;
            this.brand = brand;
            this.ValidFrom = validFrom;
            this.ValidTo = validTo;
            this.premiumFrom = premiumFrom;
            this.premiumTo = premiumTo;
        }

        public bool Equals(CommissionConfigKey other)
        {
            return username == other.username
                   && product == other.product
                   && actType == other.actType
                   && payoutType == other.payoutType
                   && policyHolderType == other.policyHolderType
                   && beneficiaryCode == other.beneficiaryCode
                   && brand == other.brand
                   && ValidFrom == other.ValidFrom
                   && ValidTo == other.ValidTo
                   && premiumFrom == other.premiumFrom
                   && premiumTo == other.premiumTo;
        }

        public override bool Equals(object obj)
        {
            return obj?.GetType() == GetType() && Equals((CommissionConfigKey)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = actType.GetHashCode();
                hashCode = (hashCode * 397) ^ payoutType.GetHashCode();
                hashCode = (hashCode * 397) ^ product.GetHashCode();
                hashCode = (hashCode * 397) ^ policyHolderType.GetHashCode();
                hashCode = (hashCode * 397) ^ (username?.GetHashCode() ?? 0);
                hashCode = (hashCode * 397) ^ (beneficiaryCode?.GetHashCode() ?? 0);
                hashCode = (hashCode * 397) ^ (brand?.GetHashCode() ?? 0);
                return hashCode;
            }
        }
    }
}
