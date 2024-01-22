using System.Collections.Generic;

namespace Libra.Contract
{
    public interface IUser
    {
        void SignIn(UserModel model);

        void SignOut();

        bool IsAuthenticated { get; }

        bool IsInRole(Role role);

        int Id { get; }

        string Username { get; }
        string FullName { get; }

        ICollection<int> ProductGroup { get; }
        ICollection<int> Supervisors { get; }
    }
}
