using System.Collections.ObjectModel;
using System.Web.Mvc;
using Libra.Contract;

namespace Libra.Web.Controllers
{
    public class CommissionController : Controller
    {
        public ICommissionService CommissionService { get; set; }
        public IUser CurrentUser { get; set; }

        //[HttpGet]
        public ActionResult List(CommissionFilter filter)
        {
            var result = CommissionService.Load(filter);

            if (!result.IsSuccess)
            {
                return HttpNotFound();
            }

            return View(result.Model);
        }

        [HttpPost]
        [ActionName("List")]
        public ActionResult LoadList(CommissionFilter filter)
        {
            return CommissionService.Load(filter).ToJsonResult();
        }

        //[HttpPost]
        //public ActionResult Save(Collection<CommissionConfigModel> model)
        //{
        //    return CommissionService.Save(model)
        //        .ToJsonResult();
        //}

        [HttpPost]
        public ActionResult Update(CommissionConfigModel model)
        {
            return CommissionService.Update(model)
                .ToJsonResult();
        }

        [HttpPost]
        public ActionResult Delete(CommissionConfigModel item)
        {
            return CommissionService.Delete(item)
                .ToJsonResult();
        }
    }
}