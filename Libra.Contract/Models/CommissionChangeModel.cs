using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Libra.Contract.Models
{
   public class CommissionChangeModel
    {
        public int Id { get; set; }

        public string[] Invoices { get; set; }

        public decimal AmountPercent { get; set; }

        public ActType ActType { get; set; }

        public PayoutType PayoutType { get; set; }

    }

    public class ComissionChangeResponseModel
    {
        public string Message { get; set; }
        public bool IsSuccess { get; set; }
    }

    public class GeneralResultModel
    {
        public List<PolicyInvoiceModel> Changes { get; set; }

        public GeneralResultModel()
        {
            Changes = new List<PolicyInvoiceModel>();
        }


        public ComissionChangeResponseModel ValidateComissionChanges(List<PolicyInvoiceModel> changes)
        {
            foreach (var item in changes)
            {
                if((int)item.Acttype==0 || (int)item.Payouttype == 0)
                {
                    
                        return new ComissionChangeResponseModel
                        {
                            IsSuccess = false,
                            Message = $" Invoice number: {item.Invoice_number} All values must be provided !"
                        };
                    
                }

                           }

            return new ComissionChangeResponseModel
            {
                IsSuccess = true,
                Message = ""
            };
        }
    }

}
