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
		InsertUOItem("43e89qs", "Red Scarves", 13);
		InsertUOItem("9124ui1", "Green Hats", 13);
		InsertUOItem("82yu63s", "Blue Mittens", 8);
		// Show Starting Data
		DisplayOutstandingOrders();
		DisplayUnorderedItems();
		//Good data
		//
		DeleteUOItem("43e89qs");
		
		//Bad Data
		//
		//DeleteUOItem("43e89qsINCORRECT");
		//DeleteUOItem(" 		");
		
		//Display Final Data
		DisplayOutstandingOrders();
		DisplayUnorderedItems();

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
//Assuming cartID is the id within the cart
public void DeleteUOItem(string PartString)
{
	List<Exception> ErrorList = new List<Exception>();
	if (string.IsNullOrWhiteSpace(PartString))
	{
		ErrorList.Add(new ArgumentNullException("VendorPartNumber cannot be null/empty/whitespace."));
	}
	UnorderedPurchaseItemCart matchingCart = UnorderedPurchaseItemCarts
		.Where(upic => upic.VendorPartNumber == PartString)
		.SingleOrDefault();
	if (matchingCart == null)
	{
		ErrorList.Add(new ArgumentException($"An Item with the ID:{PartString} does not exist"));
	}
	//after final error possible
	if (ErrorList.Count() > 0)
	{
		throw new AggregateException(ErrorList);
	}
	UnorderedPurchaseItemCarts.Remove(matchingCart);

	SaveChanges();
}

public void InsertUOItem(string partString, string description, int quantity)
{
	List<Exception> ExceptionList = new List<Exception>();
	//UnorderedPurchaseItemCart matchingCart = UnorderedPurchaseItemCarts
	//	.Where(upic => upic.CartID == cartId)
	//	.SingleOrDefault();
	//if (matchingCart == null)
	//{
	//	ExceptionList.Add( new ArgumentException($"A Temporary cart with id:{cartId} does not exist"));
	//}
	if (string.IsNullOrWhiteSpace(partString))
	{
		ExceptionList.Add(new ArgumentNullException("The given PartString cannot be a null, whitespace, or empty value"));
	}
	if (string.IsNullOrWhiteSpace(description))
	{
		ExceptionList.Add(new ArgumentNullException("The given Description cannot be a null, whitespace, or empty value"));
	}
	if (quantity <= 0)
	{
		ExceptionList.Add(new ArgumentOutOfRangeException("The given Quantity cannot be equal to or less than 0"));
	}

	if (ExceptionList.Count() > 0)
	{
		throw new AggregateException("Unable to insert item, there were errors during processing.", ExceptionList);
	}
	//no errors past this point

	UnorderedPurchaseItemCarts.Add(new UnorderedPurchaseItemCart
	{
		VendorPartNumber = partString,
		Description = description,
		Quantity = quantity
	});

	SaveChanges();
}
#endregion

#region DB Methods

public void ResetData()
{
	UnorderedPurchaseItemCarts.RemoveRange(UnorderedPurchaseItemCarts.ToList());
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