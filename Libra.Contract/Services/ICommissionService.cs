using Libra.Contract.Models;
using System.Collections.Generic;

namespace Libra.Contract
{
    public interface ICommissionService
    {
        //OperationResult<ICollection<CommissionConfigModel>> Load();
        OperationResult<ICollection<CommissionConfigModel>> Load(CommissionFilter commissionFilter);

        OperationResult Save(ICollection<CommissionConfigModel> model);
        OperationResult Update(CommissionConfigModel model);
        OperationResult Delete(CommissionConfigModel model);

        OperationResult SaveComissionChanges(GeneralResultModel model);

        OperationResult CheckInvoice(string invoice_number);


    }
}
