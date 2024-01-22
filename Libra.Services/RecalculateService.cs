using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Libra.Contract;
using Libra.Services.Database;
using Libra.Services.Helper;

namespace Libra.Services
{
    public class RecalculateService : IRecalculateService
    {
        public OperationResult<List<RecalculateInvoiceModel>> ReCalculate(List<RecalculateInvoiceModel> model)
        {
            List<RecalculateInvoiceModel> passedModel = new List<RecalculateInvoiceModel>();
            List<RecalculateInvoiceModel> notPassedModel = new List<RecalculateInvoiceModel>();

            using (var db = new LibraDb())
            {
                foreach (RecalculateInvoiceModel inv in model)
                {
                    var actCommissions = db.ActCommissions.Where(x => x.InvoiceId == inv.InvoiceNumber)?.ToList();
                    if (actCommissions != null && actCommissions.Count > 0)
                    {
                        var actIds = actCommissions?.Select(x => x.ActId).ToList();

                        var acts = db.Acts.Where(x => actIds.Contains(x.Id)).ToList();
                        
                        bool hasActiveAct = acts.Any(x => x.Status == (int)ActStatus.Approved) ||
                                            acts.Any(x => x.Status == (int)ActStatus.Paid) ||
                                            acts.Any(x => x.Status == (int)ActStatus.Sent);

                        if (!hasActiveAct)
                        {
                            db.ActCommissions.RemoveRange(actCommissions);
                            db.Payments.RemoveRange(db.Payments.Where(x => x.InvoiceId == inv.InvoiceNumber).ToList());
                            var commList = db.Commissions.Where(x => x.InvoiceId == inv.InvoiceNumber).ToList();
                            db.Commissions.RemoveRange(commList);
                            db.Invoices.RemoveRange(db.Invoices.Where(x => x.Id == inv.InvoiceNumber).ToList());
                            db.SaveChanges();
                            passedModel.Add(inv);
                            continue;
                        }
                        inv.Error = "Has active act";
                        inv.Status = "unsuccess";
                        notPassedModel.Add(inv);
                    }
                    else
                    {
                        db.Payments.RemoveRange(db.Payments.Where(x => x.InvoiceId == inv.InvoiceNumber).ToList());
                        var commList = db.Commissions.Where(x => x.InvoiceId == inv.InvoiceNumber).ToList();
                        db.Commissions.RemoveRange(commList);
                        db.Invoices.RemoveRange(db.Invoices.Where(x => x.Id == inv.InvoiceNumber).ToList());
                        db.SaveChanges();
                        passedModel.Add(inv);
                    }
                }
            }

            OperationResult<List<RecalculateInvoiceModel>> result = new OperationResult<List<RecalculateInvoiceModel>>();
            result.Model = notPassedModel;

            if (passedModel.Count > 0)
            {

                HttpClient httpClient = new HttpClient();
                result = Task.Run(async () =>
                   await httpClient.PostJsonAsync<List<RecalculateInvoiceModel>, List<RecalculateInvoiceModel>>
                   ($"{ConfigurationManager.AppSettings["IMSAPIBASEURL"]}/invoice/export", model)).Result;

                result.Model.AddRange(notPassedModel);
            }

            using (LibraDb db = new LibraDb())
            {
                foreach (var i in result.Model)
                {
                    RecalculatedInvoiceLog inv = new RecalculatedInvoiceLog();
                    inv.InvoiceId = i.InvoiceNumber;
                    inv.Status = i.Status;
                    inv.ErrorDescription = i.Error;
                    db.RecalculatedInvoiceLogs.Add(inv);
                }

                db.SaveChanges();
            }

            return result;
        }
    }
}
