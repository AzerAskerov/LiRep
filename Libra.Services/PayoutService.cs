using Libra.Contract;
using Libra.Services.Database;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;

namespace Libra.Services
{
    public class PayoutService : IPayoutService
    {
        private readonly ITranslationProvider translationProvider; 

        public PayoutService(ITranslationProvider translationProvider)
        {
            this.translationProvider = translationProvider;
        }

        public OperationResult<ICollection<string>> GetActsWithPolicyNumber(ActFilter filter)
        {

            using (var db = new LibraDb())
            {
                var actNumbers = (from inv in db.Invoices
                                  join actCom in db.ActCommissions
                                           on inv.Id equals actCom.InvoiceId into actInfo
                                  where inv.PolicyNumber == filter.PolicyNumber
                                  from actCom in actInfo.DefaultIfEmpty()
                                  select actCom.ActId).ToList();

                return new OperationResult<ICollection<string>>(actNumbers);
            }
        }

        public OperationResult<ICollection<PayoutModel>> Load(PayoutFilter filter)
        {
            //var types = filter.Types.Select(t => (int)t)
            //    .ToArray();

            if (filter.PayoutDateFrom.HasValue)
            {
                filter.PayoutDateFrom = filter.PayoutDateFrom.Value.Date;
            }
            if (filter.PayoutDateTill.HasValue)
            {
                filter.PayoutDateTill = filter.PayoutDateTill.Value.Date.AddDays(1);
            }
            

            

            using (var db = new LibraDb())
            {
                var payouts = db.Payouts
                   
                    .Where(p => ( filter.Types.Any(x=>(int)x==p.Type))
                            && (!p.PayerId.HasValue || p.PayerId == filter.PayerId)
                            && (string.IsNullOrEmpty(filter.PolicyNumber) || filter.ListActNumber.Count != 0)
                            && (!filter.ListActNumber.Any() || filter.ListActNumber.Contains(p.ActId))
                            && (filter.Status != 0)
                            && (!filter.Status.HasValue || filter.Status == p.Act.Status)
                            && (filter.Amount == 0 || filter.Amount==p.Amount)
                            && (filter.ActNumber==p.ActId || filter.ActNumber==null)
                            && (p.PayDate>=filter.PayoutDateFrom.Value || !filter.PayoutDateFrom.HasValue)
                            && (p.PayDate < filter.PayoutDateTill.Value || !filter.PayoutDateTill.HasValue)
                            && (string.IsNullOrEmpty(filter.Creator) || (p.Act.Creator.Name+ " " +p.Act.Creator.Surname).Contains(filter.Creator)))
                    .Take(50)
                    .Include(p => p.Invoices)
                    .Include(p => p.Payer)
                    .Include(p => p.Act)
                    .Include(p => p.Act.Broker)
                    .Include(p => p.Act.Creator)
                    .ToList();

                 var model = payouts
                    .Select(p => new PayoutModel
                    {
                        Id = p.Id,
                        ActId = p.ActId,
						Status = translationProvider.GetString((ActStatus)p.Act.Status),
                        Creator =  p.Act.Creator.Name + " " +p.Act.Creator.Surname ,
                        Type = (PayoutType)p.Type,
                        TypeText = translationProvider.GetString((PayoutType)p.Type),
                        Amount = p.Amount,
                        PayerId = p.PayerId,
                        PayDate = p.PayDate,
                        Payer = p.Payer?.FullName(),
                        Broker= p.Act.Broker?.FullName(),
                        Invoices = p.Invoices.Select(i => new InvoiceModel
                        {
                            Number = i.InvoiceId,
                            PolicyHolder = i.PolicyHolder,
                            PolicyNumber = i.PolicyNumber,
                            Commission = new AmountModel
                            {
                                Value = i.Amount
                            }
                        }).ToList()
                    })
                    .ToList();

                return new OperationResult<ICollection<PayoutModel>>(model);
            }

        }

        public OperationResult Pay(string actId, PayoutType type, DateTime payDate, IUser user)
        {
            using (var db = new LibraDb())
            {
                var act = db.Acts
                    .Include(a => a.Payouts)
                    .Single(a => a.Id == actId);

                var payout = act.Payouts.Single(p => p.Type == (int) type);

                payout.PayerId = user.Id;
                payout.PayDate = payDate;

                if (act.Payouts.All(p => p.PayDate.HasValue))
                {
                    act.Status = (int) ActStatus.Paid;
                }

                db.SaveChanges();

                return new OperationResult();
            }
        }

        public OperationResult<string> CreateBankFile(ICollection<PayoutModel> payouts)
        {
            var result = string.Join(
                Environment.NewLine,
                payouts.Select(p => $"{p.ActId} {p.Amount:0.00}"));

            return new OperationResult<string>(result);
        }

        public OperationResult Pay(ICollection<PayoutModel> payouts, IUser user)
        {
            using (var db = new LibraDb())
            {
                var ids = payouts.Select(p => p.Id)
                    .ToList();

                var dbPayouts = db.Payouts
                    .Where(p => ids.Contains(p.Id))
                    .Include(p => p.Act.Payouts)
                    .ToList();


                foreach (var payout in dbPayouts)
                {
                    if (payout.PayDate.HasValue
                        || payout.Type != (int) PayoutType.Bank)
                    {
                        return new OperationResult(translationProvider.GetString("INVALID_STATE"), IssueSeverity.Error);
                    }

                    payout.PayerId = user.Id;
                    payout.PayDate = DateTime.Now;

                    if (payout.Act.Payouts.All(p => p.Type == payout.Type || p.PayDate.HasValue))
                    {
                        payout.Act.Status = (int)ActStatus.Paid;
                    }
                }

                db.SaveChanges();

                return new OperationResult();
            }
        }
    }
}
