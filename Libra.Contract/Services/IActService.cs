using Libra.Contract.Models;
using System.Collections.Generic;

namespace Libra.Contract
{
    public interface IActService
    {
        OperationResult<ActModel> Create(string[] invoices, ActType type, IUser user, int? customUserID = null);

        OperationResult<ActModel> CreateFromCommissionChange(PolicyInvoiceModel model, IUser user, int? customUserID = null);

        OperationResult<ActModel> Add(string id, string[] invoices, ActType type, IUser user);

        OperationResult<ActModel> Save(ActModel act, IUser user);

        OperationResult<ActModel> Send(ActModel act, IUser user);

        OperationResult<ActModel> Approve(string id, IUser user);

        OperationResult<ActModel> Reject(ActModel act, IUser user);

        OperationResult<ActModel> Cancel(string id, IUser currentUser);

        OperationResult<ICollection<ActListItemModel>> Load(ActFilter filter);

        OperationResult<ICollection<string>> GetActsWithPolicyNumber(ActFilter filter);
        
        OperationResult<ICollection<string>> GetActsWithInvoiceNumber(ActFilter filter);

        OperationResult<ActModel> Load(string id, IUser user);

        OperationResult<ActModel> Print(string id, IUser user);
        OperationResult<byte[]> ExportToExcel(ActFilter filter);
        OperationResult<byte[]> ExportToExcel(string id, IUser user);
    }
}
