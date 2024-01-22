using System.Collections.Generic;

namespace Libra.Contract
{
    public interface IUserService
    {
        OperationResult<UserModel> Authenticate(LoginModel loginData);

        OperationResult<ICollection<UserListItemModel>> Load(UserFilter filter);

        OperationResult<UserProfileModel> Load(string username);

        OperationResult Save(UserProfileModel model);
    }
}
