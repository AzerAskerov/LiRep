using Libra.Contract;
using System;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;

namespace Libra.Web
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true)]
    public class AuthorizeRoleAttribute : AuthorizeAttribute
    {
        private readonly Role[] roles;

        public AuthorizeRoleAttribute(params Role[] roles)
        {
            this.roles = roles;
        }

        protected IUser User => DependencyResolver.Current.GetService<IUser>();

        protected override bool AuthorizeCore(HttpContextBase httpContext)
        {
            return roles.Any(User.IsInRole);
        }

        protected override void HandleUnauthorizedRequest(AuthorizationContext filterContext)
        {
            filterContext.Result = new HttpStatusCodeResult(HttpStatusCode.Forbidden);
        }
    }
}