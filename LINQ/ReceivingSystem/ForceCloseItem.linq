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
		//458
		//565
		ForceCloseOrder(458, "Company has closed", 1);
		//Bad Data
		//ForceCloseOrder(565, "", 1);
		//ForceCloseOrder(565, "Company has closed", 84);
		//ForceCloseOrder(9581, "", 84);
		//Display Final Data
		DisplayOutstandingOrders();

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
#endregion

#region Service Methods

public void ForceCloseOrder(int PurchaseOrderID, string ForceCloseReason, int EmployeeID)
{
	List<Exception> ErrorList = new List<Exception>();
	if (PurchaseOrderID <= 0)
	{
		ErrorList.Add(new ArgumentException("PurchaseOrderID cannot be equal to or less than 0"));
	}
	if (string.IsNullOrWhiteSpace(ForceCloseReason))
	{
		ErrorList.Add(new ArgumentNullException("A reason must be given to force close a purchase order."));
	}
	PurchaseOrders matchingOrder = PurchaseOrders
		.Where(po => po.PurchaseOrderID == PurchaseOrderID)
		.SingleOrDefault();
	if (matchingOrder == null)
	{
		ErrorList.Add(new ArgumentException($"An Order with the ID:{PurchaseOrderID} does not exist"));
	}
	Employees matchingEmployee = Employees
		.Where(e => e.EmployeeID == EmployeeID)
		.SingleOrDefault();
	if (matchingEmployee == null)
	{
		ErrorList.Add(new ArgumentException($"An Employee with the ID:{EmployeeID} does not exist"));
	}
	
	if (ErrorList.Count() > 0)
	{
		throw new AggregateException(ErrorList);
	}
	
	matchingOrder.Closed = true;
	matchingOrder.Notes = ForceCloseReason;

	SaveChanges();
}
#endregion

#region DB Methods

public void ResetData()
{
	PurchaseOrders
		.Where(po => po.PurchaseOrderID == 458 | po.PurchaseOrderID == 565)
		.ToList()
		.ForEach(po => 
		{
			po.Closed = false;
			po.Notes = null;
		});
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