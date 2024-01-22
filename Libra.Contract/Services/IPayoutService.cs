using System;
using System.Collections.Generic;

namespace Libra.Contract
{
    public interface IPayoutService
    {
        OperationResult<ICollection<PayoutModel>> Load(PayoutFilter filter);

        OperationResult Pay(string actId, PayoutType type, DateTime payDate, IUser user);

        OperationResult<string> CreateBankFile(ICollection<PayoutModel> payouts);

        OperationResult<ICollection<string>> GetActsWithPolicyNumber(ActFilter filter);

        OperationResult Pay(ICollection<PayoutModel> payouts, IUser user);
    }
}
