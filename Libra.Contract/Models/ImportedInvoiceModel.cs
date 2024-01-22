using System;
using System.Xml.Serialization;

namespace Libra.Contract.Models
{
    [XmlRoot("Invoices")]
    public class ImportedInvoicesModel
    {
        [XmlElement("Invoice")]
        public ImportedInvoiceModel[] Invoices { get; set; }
    }

    public class ImportedInvoiceModel
    {
        public string InvoiceNumber { get; set; }
        public string PolicyNumber { get; set; }
        public short InvoiceType { get; set; }
        public int? Product { get; set; }
        public ImportedInvoicePersonModel PolicyHolder { get; set; }
        public ImportedInvoicePersonModel Beneficiary { get; set; }
        public decimal Premium { get; set; }
        public decimal PayablePremium { get; set; }
        public decimal? PaidPremium { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime? PaidDate { get; set; }
        public ImportedInvoiceUserModel Mediator { get; set; }
        //vehicle
       public VehicleModel Vehicle { get; set; }
     
     
        public decimal? UserCommission { get; set; }
    }

    public class ImportedInvoicePersonModel
    {
        public int Type { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }
        //AddressRegion
        public string AddressRegion { get; set; }
    }

    public class ImportedInvoiceUserModel
    {
        public string Login { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        //LoginRegion prop
        public string LoginRegion { get; set; }

    }

    /*vehicle class 
      <Vehicle>
      <Brand>SCHMITZ  YARIMQOŞQU</Brand>
      <Model></Model>
      <VehicleRegion>Baki</VehicleRegion>
      <VehicleAge />
      <VehicleType>5</VehicleType>
      <EngineCapacity>0</EngineCapacity>
    </Vehicle> */
    public class VehicleModel
    {
        public string Brand { get; set; }
        public string Model { get; set; }
        public string VehicleRegion { get; set; }
        public int? VehicleAge { get; set; }
        public int? VehicleType { get; set; }
        public decimal? EngineCapacity { get; set; }

    }

     

}
