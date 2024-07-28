namespace ServicingSystem.ViewModels

{
	public class JobView
	{
        public int EmployeeId {get; set;}
        public decimal ShopRate {get; set;}
        public string VehicleIdentificationNumber {get; set;}
        public decimal TaxAmount {get; set;}
        public decimal SubTotal {get; set;}

        public DateTime JobDateIn { get; set; }
	}
}
