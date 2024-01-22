using Libra.Contract;
using Libra.Services.Database;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Libra.Services
{
    public class InvoiceMapper
    {
        private readonly ITranslationProvider translationProvider;

        public InvoiceMapper(ITranslationProvider translationProvider)
        {
            this.translationProvider = translationProvider;
        }

        internal InvoiceModel MapInvoiceModel(InvoiceProjection projection)
        {
            var invoice = projection.Invoice;

            return new InvoiceModel
            {
                Number = invoice.Id,
                PolicyNumber = invoice.PolicyNumber,
                PolicyHolder = invoice.PolicyHolder,
                Product =((Product)invoice.Product).ToString(),
                Brand =  invoice.Brand,
                Beneficiary = invoice.Beneficiary,
                Premium = invoice.Premium,
                PayablePremium = invoice.PayablePremium,
                PaidPremium = invoice.PaidPremium,
                UnpaidPremium = invoice.UnpaidPremium,
                WithheldCommission = invoice.WithheldCommission,
                CreatorId = invoice.CreatorId,
                Creator = invoice.Creator,
                IsCommissionFromIms = invoice.IsCommissionFromIms ?? false,
                Commission = new AmountModel
                {
                    Value = projection.Commissions.Sum(c => c.TotalAmount),
                    Percent = projection.Commissions.Sum(c => c.TotalPercent)
                },
                Commissions = projection.Commissions.Select(MapCommissionModel).ToList()
            };
        }

        internal CommissionModel MapCommissionModel(ICommissionView commission)
        {
            return new CommissionModel
            {
                InvoiceId = commission.InvoiceId,
                Type = (PayoutType)commission.PayoutType,
                TypeText = translationProvider.GetString((PayoutType)commission.PayoutType),
                TotalAmount = new AmountModel
                {
                    Value = commission.TotalAmount,
                    Percent = commission.TotalPercent
                },
                PaidAmount = new AmountModel
                {
                    Value = commission.PaidAmount,
                    Percent = commission.PaidPercent
                },
                UnpaidAmount = new AmountModel
                {
                    Value = commission.UnpaidAmount,
                    Percent = commission.UnpaidPercent
                },
                CalculatedAmount = commission.CustomAmount,
                CustomAmount = commission.CustomAmount,
                CustomPercent= Convert.ToDecimal(commission.CustomPercent.ToString("0.#")),
                IsManual = commission.IsManual
            };
        }
    }

    internal class InvoiceProjection
    {
        public IInvoiceView Invoice { get; set; }
        public IEnumerable<ICommissionView> Commissions { get; set; }
    }
}
