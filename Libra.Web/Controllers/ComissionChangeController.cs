using Libra.Contract;
using Libra.Contract.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Libra.Web.Controllers
{
    public class ComissionChangeController : Controller
    {
        public ICommissionService CommissionService { get; set; }

        public IInvoiceService InvoiceService { get; set; }

        public IUser CurrentUser { get; set; }


        public ActionResult Index()
        {
            bool CanAddExtraAct = CurrentUser.IsInRole(Role.CustomCommission);
            bool CanPreDefineCustomCommision = CurrentUser.IsInRole(Role.CanPredefineCommission);

            ViewBag.CanPreDefineCustomCommision = CanPreDefineCustomCommision;
            ViewBag.CanAddExtraAct = CanAddExtraAct;

            return View();
        }

        public ActionResult Load(string policy_number)
        {
            return InvoiceService.GetPolicyWithInvoices(policy_number).ToJsonResult();
        }


        public ActionResult SaveCommissionChanges(GeneralResultModel result)
        {
            return CommissionService.SaveComissionChanges(result).ToJsonResult();
        }


        public ActionResult CheckInvoice(string invoice_number)
        {
            return CommissionService.CheckInvoice(invoice_number).ToJsonResult();
        }
    }
}