//#load ".\OrderItemDetailsView.cs"
#nullable disable
using System.Collections.Generic;
namespace ReceivingSystem.ViewModels
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