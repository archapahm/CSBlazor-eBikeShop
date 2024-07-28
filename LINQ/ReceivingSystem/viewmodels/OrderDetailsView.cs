//#load ".\OrderItemDetailsView.cs"
using System.Collections.Generic;
namespace eBike
{
	public class OrderDetailsView
	{
		
		public int PurchaseOrderId { get; set; }
		public string VendorName { get; set; }
		public string VendorPhone { get; set; }
		
		public int EmployeeId { get; set; }
		
		public List<OrderItemDetailsView> ItemDetails { get; set; }

	}
}