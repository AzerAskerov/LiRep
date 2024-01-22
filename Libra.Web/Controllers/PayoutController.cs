using System;
using System.Collections.Generic;
using System.Text;
using Libra.Contract;
using System.Web.Mvc;

namespace Libra.Web.Controllers
{
    [AuthorizeRole(Role.ActPayer)]
    public class PayoutController : Controller
    {
        public IPayoutService PayoutService { get; set; }
        public IUser CurrentUser { get; set; }
        public ITranslationProvider TranslationProvider { get; set; }

        [HttpGet]
        public ActionResult List()
        {
            return View();
        }

        [HttpPost]
        public ActionResult List(PayoutFilter filter)
        {
            if (CurrentUser.IsInRole(Role.ActBankPayer))
            {
                filter.Types.Add(PayoutType.Bank);
            }
            //if (CurrentUser.IsInRole(Role.ActCashPayer))
            //{
            //    filter.Types.Add(PayoutType.Cash);
            //}
            if (CurrentUser.IsInRole(Role.ActCustomPayer))
            {
                filter.Types.Add(PayoutType.Custom);
            }
            filter.PayerId = CurrentUser.Id;
            return PayoutService.Load(filter).ToJsonResult();
        }

        [HttpPost]
        public ActionResult Pay(PayoutModel model)
        {
            return PayoutService.Pay(model.ActId, model.Type, model.PayDate ?? DateTime.Now, CurrentUser)
                .ToJsonResult();
        }

        [HttpPost]
        [AuthorizeRole(Role.ActBankPayer)]
        public ActionResult PayBank(PayoutModel payouts)
        {
            var firstpayout = new List<PayoutModel>();
            firstpayout.Add(payouts);
            return PayoutService.Pay(firstpayout, CurrentUser).ToJsonResult();
        }

        [HttpGet]
        [AuthorizeRole(Role.ActBankPayer)]
        public ActionResult UnpaidBankPayments()
        {
            var filter = new PayoutFilter
            {
                PayerId = null,
                Types = { PayoutType.Bank }
            };
            var result = PayoutService.Load(filter);

            if (result.IsSuccess && (result.Model?.Count ?? 0) == 0)
            {
                return new OperationResult(TranslationProvider.GetString("PAYOUTS_NOT_FOUND"), IssueSeverity.Error).ToJsonResult();
            }

            return result.ToJsonResult();
        }

        [HttpPost]
        [AuthorizeRole(Role.ActCreator)]
        [ActionName("GetPolicyFilterList")]
        public ActionResult GetPolicyFilterList(ActFilter filter)
        {
            return PayoutService.GetActsWithPolicyNumber(filter).ToJsonResult();
        }

        [HttpPost]
        [AuthorizeRole(Role.ActBankPayer)]
        public ActionResult ExportFile(string model)
        {
            var result = PayoutService.CreateBankFile(model.ParseJson<List<PayoutModel>>());

            if (!result.IsSuccess)
            {
                return HttpNotFound();
            }

            return File(
                Encoding.UTF8.GetBytes(result.Model), 
                "text/plain",
                $"{DateTime.Now:yyyy-MM-ddTHH-mm-ss}.txt");
        }
    }
}