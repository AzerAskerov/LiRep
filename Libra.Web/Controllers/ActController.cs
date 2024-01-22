using System.Linq;
using System.Net;
using Libra.Contract;
using System.Web.Mvc;
using System;
using Libra.Contract.Models;

namespace Libra.Web.Controllers
{
    public class ActController : Controller
    {
        public IActService ActService { get; set; }
        public IUser CurrentUser { get; set; }

        [HttpPost]
        [AuthorizeRole(Role.PrimaryActCreator)]
        public ActionResult Create(string[] invoices)
        {
            return ActService.Create(invoices, ActType.Primary, CurrentUser).ToJsonResult();
        }

        [HttpPost]
        [AuthorizeRole(Role.PrimaryActCreator, Role.UserComission)]
        public ActionResult CreateFromCommissionChange(PolicyInvoiceModel model)
        {
            model.Com = 1;
            return ActService.CreateFromCommissionChange(model, CurrentUser).ToJsonResult();
        }


        [HttpPost]
        [AuthorizeRole(Role.SecondaryActCreator)]
        public ActionResult CreateSecondary(string[] invoices)
        {
            return ActService.Create(invoices, ActType.Secondary, CurrentUser).ToJsonResult();
        }

        [HttpPost]
        [AuthorizeRole(Role.PrimaryActCreator)]
        public ActionResult Add(string id, string[] invoices)
        {
            return ActService.Add(id, invoices, ActType.Primary, CurrentUser).ToJsonResult();
        }

        [HttpPost]
        [AuthorizeRole(Role.SecondaryActCreator)]
        public ActionResult AddSecondary(string id, string[] invoices)
        {
            return ActService.Add(id, invoices, ActType.Secondary, CurrentUser).ToJsonResult();
        }

        [HttpPost]
        [AuthorizeRole(Role.ActCreator, Role.UserComission)]
        public ActionResult Preview(string model)
        {
            return View("View", model.ParseJson<ActModel>());
        }

        [HttpGet]
        [AuthorizeRole(Role.ActCreator, Role.ActCurator, Role.Underwriter, Role.ActBankPayer, Role.ActCustomPayer)]
        [ActionName("View")]
        public ActionResult ViewAct(string id)
        {
            var act = ActService.Load(id, CurrentUser);

            if (!act.IsSuccess)
            {
                return HttpNotFound();
            }

            bool hasPermissions;

            ViewBag.Admin = false;

            if (CurrentUser.IsInRole(Role.Admin) || CurrentUser.IsInRole(Role.AllActCustomTypeViewver)  || CurrentUser.IsInRole(Role.AllActBankTypeViewer))
            {
                hasPermissions = true;
                ViewBag.Admin = true;
            }

            else
            {
                hasPermissions = act.Model.CreatorId == CurrentUser.Id
                                 || act.Model.Approvals.Any(a => a.ApproverId == CurrentUser.Id)
                                 || CurrentUser.IsInRole(Role.Underwriter);
            }


            if (!hasPermissions)
            {
                return new HttpStatusCodeResult(HttpStatusCode.Forbidden);
            }

            return View("View", act.Model);
        }

        [HttpGet]
        [AuthorizeRole(Role.ActCreator, Role.ActBankPayer, Role.ActCustomPayer)]
        public ActionResult Print(string id)
        {
            var act = ActService.Load(id, CurrentUser);

            if (!act.IsSuccess)
            {
                return HttpNotFound();
            }


            if (act.Model.CreatorId != CurrentUser.Id)
            {
                return new HttpStatusCodeResult(HttpStatusCode.Forbidden);
            }

            if (act.Model.Status != ActStatus.Approved)
            {
                return new HttpStatusCodeResult(HttpStatusCode.Forbidden);
            }

            var printAct = ActService.Print(id, CurrentUser);

            return new FileContentResult(printAct.Model.PrintFileContent, printAct.Model.PrintFileType);
        }

        [HttpPost]
        [AuthorizeRole(Role.ActCreator)]
        public ActionResult Save(ActModel model)
        {
            return ActService.Save(model, CurrentUser).ToJsonResult();
        }

        [HttpPost]
        [AuthorizeRole(Role.ActCreator)]
        public ActionResult Send(ActModel model)
        {
            return ActService.Send(model, CurrentUser).ToJsonResult();
        }

        [HttpPost]
        [AuthorizeRole(Role.ActCreator)]
        public ActionResult Cancel(string id)
        {
            return ActService.Cancel(id, CurrentUser).ToJsonResult();
        }

        [HttpPost]
        [AuthorizeRole(Role.ActCurator, Role.Underwriter)]
        public ActionResult Approve(string id)
        {
            return ActService.Approve(id, CurrentUser).ToJsonResult();
        }

        [HttpPost]
        [AuthorizeRole(Role.ActCurator, Role.Underwriter)]
        public ActionResult Reject(ActModel model)
        {
            return ActService.Reject(model, CurrentUser).ToJsonResult();
        }

        [HttpGet]
        [AuthorizeRole(Role.ActCreator, Role.Underwriter, Role.ActBankPayer, Role.ActCustomPayer)]
        public ActionResult List()
        {
            return View();
        }

        [HttpPost]
        [AuthorizeRole(Role.ActCreator, Role.Underwriter, Role.ActBankPayer, Role.ActCustomPayer)]
        [ActionName("List")]
        public ActionResult LoadList(ActFilter filter)
        {
            filter.CreatorId = CurrentUser.Id;
            if (CurrentUser.IsInRole(Role.AllActCustomTypeViewver))
            {
                filter.Types.Add(PayoutType.Custom);
            }

            if(CurrentUser.IsInRole(Role.AllActBankTypeViewer))
            {
                filter.Types.Add(PayoutType.Bank);
            }

            return ActService.Load(filter).ToJsonResult();
        }

        [HttpPost]
        [ActionName("GetPolicyFilterList")]
        public ActionResult GetPolicyFilterList(ActFilter filter)
        {
            return ActService.GetActsWithPolicyNumber(filter).ToJsonResult();
        }

        [HttpPost]
        [ActionName("GetInvoiceFilterList")]
        public ActionResult GetInvoiceFilterList(ActFilter filter)
        {
            return ActService.GetActsWithInvoiceNumber(filter).ToJsonResult();
        }

        [HttpGet]
        [AuthorizeRole(Role.ActCurator, Role.Underwriter)]
        public ActionResult Approvals()
        {
            return View();
        }

        [HttpPost]
        [AuthorizeRole(Role.ActCurator, Role.Underwriter)]
        [ActionName("Approvals")]
        public ActionResult LoadApprovals(ActFilter filter)
        {
            filter.ApproverId = CurrentUser.Id;
            filter.IsUnderwriter = CurrentUser.IsInRole(Role.Underwriter);
            return ActService.Load(filter).ToJsonResult();
        }

        [HttpPost]
        [AuthorizeRole(Role.ActCreator, Role.ActCurator, Role.Underwriter, Role.ActBankPayer, Role.ActCustomPayer)]
        [ActionName("ExportExcel")]
        public ActionResult ExportExcel(ActFilter filter)
        {
            filter.CreatorId = CurrentUser.Id;
            var exportExcelAct = ActService.ExportToExcel(filter);
            string handle = Guid.NewGuid().ToString();
            TempData[handle] = exportExcelAct.Model;
            return new JsonResult() { Data = new { FileGuid = handle } };
            //return File(exportExcelAct.Model, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "test.xlsx");
        }

        [HttpGet]
        [AuthorizeRole(Role.ActCreator, Role.ActCurator, Role.Underwriter, Role.ActBankPayer, Role.ActCustomPayer)]
        [ActionName("Download")]
        public ActionResult Download(string fileGuid)
        {
            if (TempData[fileGuid] != null)
            {
                byte[] data = TempData[fileGuid] as byte[];
                return File(data, "application/vnd.ms-excel", "Acts.xlsx");
            }
            else
            {
                return new EmptyResult();
            }
        }

        [HttpGet]
        [AuthorizeRole(Role.ActCreator, Role.ActCurator, Role.Underwriter, Role.ActBankPayer, Role.ActCustomPayer)]
        [ActionName("ExportInvoiceList")]
        public ActionResult ExportInvoiceList(string id)
        {
            var exportExcelAct = ActService.ExportToExcel(id, CurrentUser);
            return File(exportExcelAct.Model, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "ActInvoices.xlsx");
        }
    }
}