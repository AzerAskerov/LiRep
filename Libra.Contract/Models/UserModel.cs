using System;
using System.Collections.Generic;

namespace Libra.Contract
{
    public class UserModel
    {
        public UserModel()
        {
            Supervisors = new List<int>();
            ProductGroup = new List<int>();
        }

        public int Id { get; set; }

        public string Username { get; set; }

        public string Name { get; set; }

        public string Surname { get; set; }

        public Role Role { get; set; }

        public ICollection<int> Supervisors { get; set; }

        public ICollection<int> ProductGroup { get; set; }
    }

    [Flags]
    public enum Role
    {
        None = 0,

        InvoiceViewer = 1,
        UnpaidInvoiceViewer = 2 | InvoiceViewer,
        ForeignInvoiceViewer = 4 | InvoiceViewer,
        SecondaryInvoiceViewer = 8,

        ActCreator = 16,
   
        PrimaryActCreator = 32 | ActCreator | InvoiceViewer,
        CustomActCreator = 64 | PrimaryActCreator,
        ApprovedActCreator = 128 | PrimaryActCreator,
        ForeignActCreator = 256 | PrimaryActCreator,
        SecondaryActCreator = 512 | ActCreator | SecondaryInvoiceViewer | PrimaryActCreator,

        ActPayer = 1024,
        ActBankPayer = 2048 | ActPayer,
        ActCashPayer = 4096 | ActPayer,
        ActCustomPayer = 8192 | ActCashPayer,

        ActCurator = 16384,

        Underwriter = 32768,

        UserAdmin = 65536,

        SupervisorInvoiceViewer = 131072 | PrimaryActCreator,


        Admin =  UnpaidInvoiceViewer 
               | ForeignActCreator
               | SecondaryInvoiceViewer
               | CustomActCreator
               | ApprovedActCreator
               | SecondaryActCreator
               | ActCurator 
               | ActBankPayer
               | ActCashPayer
               | ActCustomPayer
               | Underwriter
               | UserAdmin
               ,

        Broker = 262144,

        UserComission = 524288 | Underwriter | ActCreator,

        AllActBankTypeViewer = 1048576 | ActBankPayer,

        AllActCustomTypeViewver = 2097152 | ActCustomPayer,


        NewAdmin = 489398 | InvoiceViewer | SecondaryInvoiceViewer | UserComission,

        CanPredefineCommission = 4194304,
        CustomCommission= 8388608


    }
}