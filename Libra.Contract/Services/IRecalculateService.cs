using System.Collections.Generic;

namespace Libra.Contract
{
    public interface IRecalculateService
    {
        OperationResult<List<RecalculateInvoiceModel>> ReCalculate(List<RecalculateInvoiceModel> model);
    }
}
