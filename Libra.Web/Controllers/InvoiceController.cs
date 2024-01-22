using Libra.Contract;
using System.Web.Mvc;

namespace Libra.Web.Controllers
{
    public class InvoiceController : Controller
    {
        public IInvoiceService InvoiceService { get; set; }

        public IUser CurrentUser { get; set; }

        [HttpGet]
        [AuthorizeRole(Role.InvoiceViewer)]
        public ActionResult List()
        {
            bool isReadOnly = CurrentUser.IsInRole(Role.NewAdmin);

            return View(new InvoiceListModel
            {
                ModuleId = NavigationModule.INVOICES,
                ReadOnly = isReadOnly
            });
        }

        [HttpGet]
        [AuthorizeRole(Role.SecondaryInvoiceViewer)]
        public ActionResult ListSecondary()
        {
            bool isReadOnly = CurrentUser.IsInRole(Role.NewAdmin);

            return View("List", new InvoiceListModel
            {
                ModuleId = NavigationModule.SECONDARY_INVOICES,
                ReadOnly = isReadOnly
            });
        }

        [HttpPost]
        [AuthorizeRole(Role.InvoiceViewer)]
        public ActionResult Load(InvoiceFilter model)
        {
            model.UserIds.Add(CurrentUser.Id);

            if (CurrentUser.IsInRole(Role.ForeignInvoiceViewer))
                model.UserIds.Clear();

            model.Type = ActType.Primary;

            return InvoiceService.Load(model)
                .ToJsonResult();
        }

        [HttpPost]
        [AuthorizeRole(Role.SecondaryInvoiceViewer)]
        public ActionResult LoadSecondary(InvoiceFilter model)
        {
            model.UserIds.Clear();
            model.Type = ActType.Secondary;

            return InvoiceService.Load(model)
                .ToJsonResult();
        }


        [HttpPost]
        public ActionResult HideInvoices(string[] invoices)
        {
            return InvoiceService.HideInvoices(invoices).ToJsonResult();
        }

        [HttpPost]
        [AuthorizeRole(Role.InvoiceViewer, Role.SecondaryInvoiceViewer)]
        public ActionResult Syncronize()
        {
            InvoiceService.Syncronize();
            return new OperationResult().ToJsonResult();
        }
    }
}