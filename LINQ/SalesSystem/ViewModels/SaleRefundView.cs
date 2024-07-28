namespace eBike
{
	public class SaleRefundView
	{
		public int SaleId {get; set;}
		public int EmployeeId {get; set;}
		public decimal TaxAmount {get;set;}
		public decimal SubTotal {get; set;}
		public int DiscountPercent {get; set;}
	}
}