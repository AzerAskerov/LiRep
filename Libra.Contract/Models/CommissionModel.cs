using System;
using System.Collections.Generic;

namespace Libra.Contract
{
    public class CommissionModel
    {
        public string InvoiceId { get; set; }
        public PayoutType Type { get; set; }
        public string TypeText { get; set; }
        public AmountModel TotalAmount { get; set; }
        public AmountModel PaidAmount { get; set; }
        public AmountModel UnpaidAmount { get; set; }
        public decimal CalculatedAmount { get; set; }
        public decimal CustomAmount { get; set; }
        public decimal CustomPercent { get; set; }
        public bool IsManual { get; set; }
    }

    public class AmountModel
    {
        public decimal Value { get; set; }
        public decimal Percent { get; set; }
    }

    public class CommissionConfigModel
    {
        public int? Id { get; set; }
        public int ActType { get; set; }
        public int PayoutType { get; set; }
        public int? Product { get; set; }
        public int? Brand { get; set; }
        public int? PolicyHolderType { get; set; }
        public string Username { get; set; }
        public string BeneficiaryCode { get; set; }
        public decimal AmountFixed { get; set; }
        public decimal AmountPercent { get; set; }
        public decimal? AmountMin { get; set; }
        public decimal? AmountMax { get; set; }
        public DateTime ValidFrom { get; set; }
        public DateTime ValidTo { get; set; }
        public decimal PremiumFrom { get; set; }
        public decimal PremiumTo { get; set; }
        public Dictionary<int?, string> Brands { get; set; } = new Dictionary<int?, string>();
        public int? VehicleType { get; set; }
        public decimal? EngineCapacityFrom { get; set; }
        public decimal? EngineCapacityTo { get; set; }
    }

    public enum VehicleType
    {
        Bus = 1,
        Truck = 2,
        Passenger = 3,
        Motorcycle = 4,
        Trailer = 5,
        Specialized = 6,
        ElectricCar = 7
    }
}
