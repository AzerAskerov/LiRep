using Libra.Contract;
using System.Linq;
using System.Net;
using System.Web.Mvc;

namespace Libra.Web.Controllers
{
    public class HomeController : Controller
    {
        public IUser CurrentUser { get; set; }

        public ActionResult Index()
        {
            var module = Navigation.Modules.Values.FirstOrDefault(m => m.Roles.Any(r => CurrentUser.IsInRole(r)));

            return module == null
                ? (ActionResult)new HttpStatusCodeResult(HttpStatusCode.Forbidden)
                : Redirect(module.Url); 
        }
    }
}