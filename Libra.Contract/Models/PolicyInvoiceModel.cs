using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Libra.Contract.Models
{
  public  class PolicyInvoiceModel
    {
       

        
        public string Policy_number { get; set; }
        public string Invoice_number { get; set; }
        public string Client_name { get; set; }
        public DateTime Start_date { get; set; }

        public DateTime End_date { get; set; }

        public string Invoice_guid { get; set; }

        public int Status_code { get; set; }

        public decimal Amount_paid { get; set; }

        public decimal Amount_premium { get; set; }

        public string Client_guid { get; set; }

        public string Client_code { get; set; }

        public DateTime Create_time { get; set; }

        public string Type_code { get; set; }

        public int Lob_oid { get; set; }

        public decimal Amount { get; set; }

        public ActType Acttype { get; set; }

        public PayoutType Payouttype { get; set; }

        public int Com { get; set; } = 1;

        public PolicyInvoiceModel()
        {
            this.Amount = -1;
        }
    }
   
}
