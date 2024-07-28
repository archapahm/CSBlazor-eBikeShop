<Query Kind="Program">
  <Connection>
    <ID>21b7c6a4-0aca-426d-82fe-3fcea8183a92</ID>
    <NamingServiceVersion>2</NamingServiceVersion>
    <Persist>true</Persist>
    <Driver Assembly="(internal)" PublicKeyToken="no-strong-name">LINQPad.Drivers.EFCore.DynamicDriver</Driver>
    <Server>.</Server>
    <Database>eBike_DMIT2018</Database>
    <DriverData>
      <EncryptSqlTraffic>True</EncryptSqlTraffic>
      <PreserveNumeric1>True</PreserveNumeric1>
      <EFProvider>Microsoft.EntityFrameworkCore.SqlServer</EFProvider>
    </DriverData>
  </Connection>
  <Namespace>eBike</Namespace>
  <IncludeUncapsulator>false</IncludeUncapsulator>
</Query>

#load ".\ViewModels\*.cs"
//#load ".\InsertUOItem"

using eBike;
using System.Collections;

void Main()
{
	try
	{
		// Init Data
		ResetData();
		DisplayOutstandingOrders();
		DisplayOrderDetails(458);
		//run
		OrderDetailsView orderDetails = GetOrderDetails(458);
		orderDetails.ItemDetails[0].ReceivedQuantity = 50;
		orderDetails.ItemDetails[0].ReturnQuantity = 1;
		orderDetails.ItemDetails[0].ReturnReason = "Bad quality";
		orderDetails.ItemDetails[1].ReceivedQuantity = 50;
		orderDetails.ItemDetails[1].ReturnQuantity = 2;
		orderDetails.ItemDetails[1].ReturnReason = "Mislabeled Item";
		orderDetails.ItemDetails[2].ReceivedQuantity = 50;
		orderDetails.ItemDetails[2].ReturnQuantity = 1;
		orderDetails.ItemDetails[2].ReturnReason = "Broken in shipping";

		ReceiveOrder(orderDetails);
		//Bad Data
		//orderDetails = GetOrderDetails(458);
		//orderDetails.ItemDetails[0].ReturnQuantity = 1;
		//orderDetails.ItemDetails[0].ReturnReason = "";
		//orderDetails.ItemDetails[1].ReturnQuantity = 800;
		//orderDetails.ItemDetails[1].ReturnReason = "Mislabeled Item";
		//orderDetails.ItemDetails[2].ReturnQuantity = 1;
		//orderDetails.ItemDetails[2].ReturnReason = "Broken in shipping";
		//ReceiveOrder(orderDetails);
		//Display Final Data
		DisplayOutstandingOrders();
		DisplayOrderDetails(458);

	}
	catch (AggregateException ex)
	{
		//ex.Message.Dump();
		foreach (var error in ex.InnerExceptions)
		{
			error.Message.Dump();
		}
	}
	//catch (Exception ex)
	//{
	//	GetInnerException(ex).Message.Dump();
	//}
}

#region Query Methods
public OrderDetailsView GetOrderDetails(int OrderId)
{
	return PurchaseOrders
		.Where(po => po.PurchaseOrderID == OrderId)
		.Select(po => new OrderDetailsView
		{
			PurchaseOrderId = po.PurchaseOrderID,
			VendorName = po.Vendor.VendorName,
			VendorPhone = po.Vendor.Phone,
			EmployeeId = po.EmployeeID,
			ItemDetails = PurchaseOrderDetails
				.Where(pod => pod.PurchaseOrderID == OrderId)
				.Select(pod => new OrderItemDetailsView
				{
					PurchaseOrderDetailsID = pod.PurchaseOrderDetailID,
					PartID = pod.PartID,
					Description = pod.Part.Description,
					Quantity = pod.Quantity,
					ReceivedQuantity = pod
						.ReceiveOrderDetails
						.Single(rod => rod.PurchaseOrderDetailID == pod.PurchaseOrderDetailID)
						.QuantityReceived,
					ReturnQuantity = pod
						.ReturnedOrderDetails
						.Single(rod => rod.PurchaseOrderDetailID == pod.PurchaseOrderDetailID)
						.Quantity,
					ReturnReason = pod
						.ReturnedOrderDetails
						.Single(rod => rod.PurchaseOrderDetailID == pod.PurchaseOrderDetailID)
						.Reason
				})
				.ToList()
		})
		.Single();
}
public List<OutstandingOrdersItemView> GetOutstandingOrders(List<PurchaseOrders> purchaseOrders)
{
	return PurchaseOrders
		.Where(po => po.Closed == false)
		.Select(po => new OutstandingOrdersItemView
		{
			PurchaseOrderId = po.PurchaseOrderID,
			OrderDate = po.OrderDate,
			VendorName = po.Vendor.VendorName,
			Phone = po.Vendor.Phone
		})
		.ToList();
}
public List<UnOrderedItemView> GetUnOrderedItems(int cartId)
{
	return UnorderedPurchaseItemCarts
		.Where(upic => upic.CartID == cartId)
		.Select(upic => new UnOrderedItemView
		{
			PartIDString = upic.VendorPartNumber,
			Description = upic.Description,
			Quantity = upic.Quantity
		})
		.ToList();
}
public void DisplayOutstandingOrders()
{
	GetOutstandingOrders(PurchaseOrders.ToList()).Dump();
}
public void DisplayUnorderedItems()
{
	UnorderedPurchaseItemCarts.Dump();
}
public void DisplayOrderDetails(OrderDetailsView order)
{
	order.Dump();
}
public void DisplayOrderDetails(int orderId)
{
	GetOrderDetails(458).Dump();
}
#endregion

#region Service Methods
public void ReceiveOrder(OrderDetailsView orderDetails)
{
	List<Exception> Errorlist = new List<Exception>();

	PurchaseOrders matchingOrder = PurchaseOrders
		.Where(po => po.PurchaseOrderID == orderDetails.PurchaseOrderId)
		.SingleOrDefault();
	if (matchingOrder == null)
	{
		Errorlist.Add(new ArgumentException("PurchaseOrderID must already exist"));
	}
	Employees matchingEmployee = Employees
		.Where(e => e.EmployeeID == orderDetails.EmployeeId)
		.SingleOrDefault();
	if (matchingEmployee == null)
	{
		Errorlist.Add(new ArgumentException("An employee with the given ID must does not exist"));
	}
	
	

	ReceiveOrders newOrder = new ReceiveOrders
	{
		EmployeeID = orderDetails.EmployeeId,
		PurchaseOrder = PurchaseOrders.Where(p => p.PurchaseOrderID == orderDetails.PurchaseOrderId).Single(),
		ReceiveDate = DateTime.Now
	};
	
	
	List<ReceiveOrderDetails> receiveDetailsToSave = new List<ReceiveOrderDetails>();
	List<ReturnedOrderDetails> returnDetailsToSave = new List<ReturnedOrderDetails>();
	orderDetails.ItemDetails.ForEach(id =>
	{
		ReceiveOrderDetails newReceiveOrderDetails = new ReceiveOrderDetails
		{
			ReceiveOrder = newOrder,
			PurchaseOrderDetailID = id.PurchaseOrderDetailsID,
			QuantityReceived = id.ReceivedQuantity.GetValueOrDefault(),
			
		};
		ReturnedOrderDetails newReturnOrderDetails = new ReturnedOrderDetails
		{
			PurchaseOrderDetailID = id.PurchaseOrderDetailsID,
			ReceiveOrder = newOrder,
			ItemDescription = Parts.Single(p => p.PartID == id.PartID).Description,
			VendorPartNumber = PurchaseOrderDetails.Single(pod => pod.PurchaseOrderDetailID == id.PurchaseOrderDetailsID).VendorPartNumber,
			Quantity = id.ReturnQuantity.Value,
			Reason = id.ReturnReason
		};
		if (newReturnOrderDetails.Quantity > id.Quantity)
		{
			Errorlist.Add(new ArgumentException("You cannot return more items than you currently have"));
		}
		receiveDetailsToSave.Add(newReceiveOrderDetails);
		returnDetailsToSave.Add(newReturnOrderDetails);
		
		
		
	});

	if (Errorlist.Count() > 0)
	{
		throw new AggregateException(Errorlist);
	}
	ReceiveOrders.Add(newOrder);
	ReceiveOrderDetails.AddRange(receiveDetailsToSave);
	ReturnedOrderDetails.AddRange(returnDetailsToSave);
	
	SaveChanges();
}
#endregion

#region DB Methods

public void ResetData()
{
	OrderDetailsView orderDetails = GetOrderDetails(458);
	orderDetails.ItemDetails[0].ReturnQuantity = null;
	orderDetails.ItemDetails[0].ReturnReason = null;
	orderDetails.ItemDetails[1].ReturnQuantity = null;
	orderDetails.ItemDetails[1].ReturnReason = null;
	orderDetails.ItemDetails[2].ReturnQuantity = null;
	orderDetails.ItemDetails[2].ReturnReason = null;
	SaveChanges();
}

#endregion

#region Misc
private Exception GetInnerException(Exception ex)
{
	while (ex.InnerException != null)
		ex = ex.InnerException;
	return ex;
}
#endregion

// Ability to take 2 date time objects (eg 2:00pm and 4:00pm) and provide the difference in minutes
// to unix time, find difference, modulus to success
//DateTime currentTime = DateTime.UtcNow;
//long unixTime = ((DateTimeOffset)currentTime).ToUnixTimeSeconds();