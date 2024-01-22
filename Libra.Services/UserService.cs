using Libra.Contract;
using Libra.Services.Database;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace Libra.Services
{
    internal class UserService : IUserService
    {
        private readonly ITranslationProvider translationProvider;

        public UserService(ITranslationProvider translationProvider)
        {
            this.translationProvider = translationProvider;
        }

        public OperationResult<UserModel> Authenticate(LoginModel model)
        {
            if (string.IsNullOrEmpty(model?.Username) || string.IsNullOrEmpty(model.Password))
            {
                return new OperationResult<UserModel>(translationProvider.GetString("ACCOUNT_LOGIN_INVALID"), IssueSeverity.Error);
            }

            using (var db = new LibraDb())
            {
                var user = db.Users.FirstOrDefault(u => model.Username.Equals(u.Username, StringComparison.OrdinalIgnoreCase));

                if (user == null || user.Password != GetPasswordHash(model.Password))
                {
                    return new OperationResult<UserModel>(translationProvider.GetString("ACCOUNT_LOGIN_INVALID"), IssueSeverity.Error);
                }





                return new OperationResult<UserModel>
                {
                    Model = new UserModel
                    {
                        Id = user.Id,
                        Username = model.Username,
                        Name = user.Name,
                        Surname = user.Surname,
                        Role = (Role)user.Role,
                        Supervisors = user.Supervisors?.Split(',').Select(int.Parse).ToList() ?? new List<int>(),
                        ProductGroup =  user.ProductGroup?.Split(',').Select(int.Parse).ToList() ?? new List<int>()
                    }
                };
            }
        }

        public OperationResult<ICollection<UserListItemModel>> Load(UserFilter filter)
        {
            using (var db = new LibraDb())
            {
                var users = db.Users
                    .Select(u => new UserListItemModel
                    {
                        Id = u.Id,
                        Username = u.Username,
                        Name = u.Name,
                        Surname = u.Surname
                    })
                    .ToList();

                return new OperationResult<ICollection<UserListItemModel>>(users);
            }
        }

        public OperationResult<UserProfileModel> Load(string username)
        {
            using (var db = new LibraDb())
            {
                var user = db.Users
                    .FirstOrDefault(u => u.Username.Equals(username, StringComparison.OrdinalIgnoreCase));

                if (user == null)
                {
                    return new OperationResult<UserProfileModel>(
                        translationProvider.GetString("USER_NOT_FOUND"), 
                        IssueSeverity.Error);
                }

                var model = new UserProfileModel
                {
                    Id = user.Id,
                    Username = user.Username,
                    Name = user.Name,
                    Surname = user.Surname
                };

                return new OperationResult<UserProfileModel>(model);
            }
        }

        public OperationResult Save(UserProfileModel model)
        {
            return new OperationResult(translationProvider.GetString("USER_SAVED"), IssueSeverity.Success);
        }


        private const string HASH_SECRET_KEY = "15042CEA-9F65-47C2-B7B6-0DAD6ED4CB8D";
        private static string GetPasswordHash(string password)
        {
            using (var hash = SHA256.Create())
            {
                return string.Join("", hash
                  .ComputeHash(Encoding.ASCII.GetBytes($"{password}-{HASH_SECRET_KEY}"))
                  .Select(item => item.ToString("x2")));
            }
        }
    }

    internal static class UserExtensions
    {
        internal static string FullName(this DbUser user)
        {
            return $"{user.Name} {user.Surname}";
        }
    }
}
