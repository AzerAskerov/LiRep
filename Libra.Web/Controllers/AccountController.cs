using Libra.Contract;
using System.Web.Mvc;

namespace Libra.Web.Controllers
{
    public class AccountController : Controller
    {
        public IUserService UserService { get; set; }
        public IUser CurrentUser { get; set; }

        [HttpGet]
        [AllowAnonymous]
        public ActionResult Login(string returnUrl)
        {
            if (User?.Identity.IsAuthenticated ?? false)
            {
                return Redirect($"~/{returnUrl}");
            }
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        public ActionResult Login(LoginModel model)
        {
            var result = UserService.Authenticate(model);

            if (result.IsSuccess)
            {
                CurrentUser.SignIn(result.Model);
            }

            return result.ToJsonResult();
        }

        [HttpGet]
        [ActionName("Profile")]
        public ActionResult ProfileAction()
        {
            return View();
        }

        [HttpGet]
        [AllowAnonymous]
        public ActionResult Logout()
        {
            CurrentUser.SignOut();
            return Redirect("~/");
        }
    }
}