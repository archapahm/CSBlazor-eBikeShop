namespace SalesSystem.ViewModels
{
	public class SaleRefundDetailView
	{
		public int PartID {get; set;}
		public string Description{get;set;}
		public int OriginalQuantity{get;set;}
		public decimal SellingPrice {get; set;}
		public int ReturnQuantity {get; set;}
		public bool Refundable {get; set;}
		public int Quantity {get; set;}
		public string Reason {get; set;}
	}
}