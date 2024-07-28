namespace eBike
{
	public class OrderItemDetailsView
	{
		public int PurchaseOrderDetailsID { get; set; }
		public int PartID { get; set; }
		public string Description { get; set; }

		public int Quantity { get; set; }
		public int? ReceivedQuantity { get; set; }
		public int? OutstandingQuantity
		{
			get 
			{
				if (Quantity - ReceivedQuantity < 0)
				{
					return 0;
				} else
				{
					return Quantity - ReceivedQuantity;
				}
				
			}
		}
		
		public int? ReturnQuantity { get; set; }
		public string? ReturnReason { get; set; }
	}
}