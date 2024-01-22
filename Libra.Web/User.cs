using Libra.Contract;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Web;
using System.Web.Mvc;

namespace Libra.Web
{
    public class User : IUser
    {
        public void SignIn(UserModel model)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Sid, model.Id.ToString()),
                new Claim(ClaimTypes.Upn, model.Username),

                new Claim(ClaimTypes.NameIdentifier, model.Username),
                new Claim(ClaimTypes.Name, model.Name),
                new Claim(ClaimTypes.Surname, model.Surname),
                new Claim(ClaimTypes.Role, $"{(int)model.Role}")
                
            };

            claims.AddRange(model.Supervisors.Select(s => new Claim(ClaimTypes.Actor, $"{s}")));
            claims.AddRange(model.ProductGroup.Select(s => new Claim("ProductGroup", $"{s}")));

            var identity = new ClaimsIdentity(claims, Config.AuthenticationType);

            HttpContext.Current.Request.GetOwinContext().Authentication.SignIn(identity);
        }

        public void SignOut()
        {
            HttpContext.Current.Request.GetOwinContext().Authentication.SignOut();
        }

        public bool IsAuthenticated => Identity?.IsAuthenticated ?? false;

        public bool IsInRole(Role role)
        {
            return ((Role)GetClaimValue<int>(ClaimTypes.Role) & role) == role;
        }

        public int Id => GetClaimValue<int>(ClaimTypes.Sid);

        public string Username => GetClaimValue(ClaimTypes.Upn);

        public string FullName => $"{GetClaimValue(ClaimTypes.Name)} {GetClaimValue(ClaimTypes.Surname)}";

        public ICollection<int> Supervisors => GetAllClaimValues<int>(ClaimTypes.Actor);

        public ICollection<int> ProductGroup => GetAllProductGroups<int>("ProductGroup") ?? new int[0];

        //public string ProductList => GetClaimValue("ProductList");

        public static IUser Current => DependencyResolver.Current.GetService<IUser>();

        private string GetClaimValue(string claimType)
        {
            return GetClaimValue<string>(claimType);
        }

        private T GetClaimValue<T>(string claimType)
        {
            var identity = Identity;

            if (!IsAuthenticated || !identity.HasClaim(c => c.Type == claimType))
            {
                return default(T);
            }

            return identity
                .FindFirst(c => c.Type == claimType)
                .Value
                .Cast<T>();
        }

        private T[] GetAllClaimValues<T>(string claimType)
        {
            var identity = Identity;

            if (!IsAuthenticated || !identity.HasClaim(c => c.Type == claimType))
            {
                return new T[0];
            }

            return identity
                .FindAll(c => c.Type == claimType)
                .Select(c => c.Value.Cast<T>())
                .ToArray();
        }

        private T[] GetAllProductGroups<T>(string claimType)
        {
            var identity = Identity;

            if (!IsAuthenticated || !identity.HasClaim(c => c.Type == claimType))
            {
                return new T[0];
            }

            return identity
                .FindAll(c => c.Type == claimType)
                .Select(c => c.Value.Cast<T>())
                .ToArray();
        }

        private static ClaimsIdentity Identity => HttpContext.Current.Request.GetOwinContext().Authentication.User?.Identity as ClaimsIdentity;

    }
}