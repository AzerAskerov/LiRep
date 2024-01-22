using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Libra.Contract;

namespace Libra.Web.Controllers
{
    public class RecalculateController : Controller
    {
        // GET: Recalculate
        public IRecalculateService RecalculateService { get; set; }

        public IUser CurrentUser { get; set; }

        [HttpGet]
        [AuthorizeRole(Role.UserAdmin)]
        public ActionResult List()
        {
            return View(new RecalculateInvoiceListModel
            {
                ModuleId = NavigationModule.RECALCULATE
            });
        }


        [HttpPost]
        [AuthorizeRole(Role.UserAdmin)]
        public ActionResult ProcessInvoices(List<RecalculateInvoiceModel> invoices)
        {
            return RecalculateService.ReCalculate(invoices).ToJsonResult();
        }
    }
}