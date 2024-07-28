using System;
namespace ReceivingSystem.ViewModels 
{
	public class OutstandingOrdersItemView
	{
		public int PurchaseOrderId { get; set; }
		public DateTime? OrderDate { get; set; }
		public string VendorName { get; set; }
		public string Phone { get; set; }
	}
}
