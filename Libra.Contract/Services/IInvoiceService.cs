using Libra.Contract.Models;
using System.Collections.Generic;

namespace Libra.Contract
{
    public interface IInvoiceService
    {
        void Syncronize();

        void ImportInvoices(string xml);

        OperationResult<InvoiceViewModel> Load(InvoiceFilter filter);

        OperationResult<List<PolicyInvoiceModel>> GetPolicyWithInvoices(string policyNumber);

        OperationResult<InvoiceModel> HideInvoices(string[] invoices);

    }
}
