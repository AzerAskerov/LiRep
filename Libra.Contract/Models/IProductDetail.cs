using System.Collections.Generic;

namespace Libra.Contract
{
    public interface IProductDetail
    {
         int Id { get; }

         int GroupId { get; }

         ICollection<int> ProductIds { get; }

    }
}
