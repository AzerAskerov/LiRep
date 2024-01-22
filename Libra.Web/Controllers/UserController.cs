using Libra.Contract;
using System.Web.Mvc;

namespace Libra.Web.Controllers
{
    [AuthorizeRole(Role.UserAdmin)]
    public class UserController : Controller
    {
        public IUserService UserService { get; set; }

        [HttpGet]
        public ActionResult List()
        {
            return View();
        }

        [HttpPost]
        public ActionResult List(UserFilter filter)
        {
            return UserService.Load(filter)
                .ToJsonResult();
        }

        [HttpPost]
        public ActionResult Save(UserProfileModel model)
        {
            return UserService.Save(model)
                .ToJsonResult();
        }

        [HttpGet]
        [ActionName("Profile")]
        public ActionResult ViewProfile(string id)
        {
            var result = UserService.Load(id);

            if (!result.IsSuccess)
            {
                return HttpNotFound();
            }
             
            return View(result.Model);
        }
    }
}