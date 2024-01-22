using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Libra.Contract;
using Libra.Contract.Models;
using Libra.Services.Database;

namespace Libra.Services
{
    public class ProductDetailService : IProductDetailService
    {
        public OperationResult<ProductDetailModel> Load(int id)
        {
            using (var db = new LibraDb())
            {
                var detail = db.ProductDetails
                    .Where(g => g.GroupId == id)
                    .FirstOrDefault(g => g.Id == id);


                var model = MapProductDetailModel(detail);
                return new OperationResult<ProductDetailModel>(model);
            }
        }

          private ProductDetailModel MapProductDetailModel(DbProductDetail detail)
            {

            var model = new ProductDetailModel
            {
                Id = detail.Id,
                GroupId = detail.GroupId,
                ProductIds = detail.ProductIds?.Split(',').Select(int.Parse).ToList() ?? new List<int>()
            };

            return model;
            }
    }
}
