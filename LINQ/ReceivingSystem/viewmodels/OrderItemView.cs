using System;

namespace eBike 
{
	public class OrderItemView
	{
		public int PurchaseOrderId { get; set; }
		public System.DateTime OrderDate { get; set; }
		public string VendorName { get; set; }
		public string VendorPhone { get; set; }
	}
}