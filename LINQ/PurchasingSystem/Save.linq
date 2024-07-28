<Query Kind="Program">
  <Connection>
    <ID>016caf36-4a94-4b73-bd48-e3474902cf77</ID>
    <NamingServiceVersion>2</NamingServiceVersion>
    <Persist>true</Persist>
    <Driver Assembly="(internal)" PublicKeyToken="no-strong-name">LINQPad.Drivers.EFCore.DynamicDriver</Driver>
    <Server>.</Server>
    <DisplayName>eBikeEntity</DisplayName>
    <Database>eBike_DMIT2018</Database>
    <DriverData>
      <EncryptSqlTraffic>True</EncryptSqlTraffic>
      <PreserveNumeric1>True</PreserveNumeric1>
      <EFProvider>Microsoft.EntityFrameworkCore.SqlServer</EFProvider>
      <EFVersion>6.0.12</EFVersion>
      <TrustServerCertificate>True</TrustServerCertificate>
    </DriverData>
  </Connection>
</Query>

#load ".\ViewModels\*.cs"

using eBike;

void Main()
{
	try
	{
		// Reset Data
		ResetData(false);

		// Driver Routine
		PurchaseOrderView purchaseOrder = null;
		int vendorIDWithOutActiveOrder = CreateVendor("Suggested Ltd");
		int vendorIDWithActiveOrder = CreateVendor("Current Active Ltd");
		
		DisplayOrders();
		DisplayInventory(vendorIDWithActiveOrder);

		// --- --- --- --- --- --- --- --- --- ---
		// START - BAD DATA FOR SAVING PURCHASE ORDER
		// --- --- --- --- --- --- --- --- --- ---
		//Console.WriteLine("Creating purchase order using bad data...");
		//purchaseOrder = CreateBadPurchaseOrder(vendorIDWithActiveOrder);
		// --- --- --- --- --- ---
		// END - BAD DATA FOR SAVING PURCHASE ORDER
		// --- --- --- --- --- ---

		// --- --- --- --- --- --- --- --- --- ---
		// START - GOOD DATA FOR SAVING PURCHASE ORDER
		// --- --- --- --- --- --- --- --- --- ---
		//Console.WriteLine("Creating purchase order using good data...");
		//purchaseOrder = CreateGoodPurchaseOrder(vendorIDWithActiveOrder);
		//GetVendorInventory(vendorIDWithActiveOrder).Dump();
		// --- --- --- --- --- --- --- --- --- ---
		// END - GOOD DATA FOR SAVING PURCHASE ORDER
		// --- --- --- --- --- --- --- --- --- ---

		// --- --- --- --- --- --- --- --- --- ---
		// START - PLACE ORDER
		// --- --- --- --- --- --- --- --- --- ---
		//Console.WriteLine("Placing purchase order...");
		//purchaseOrder = CreateGoodPurchaseOrder(vendorIDWithActiveOrder);
		//purchaseOrder.OrderDate = DateTime.Now;
		// --- --- --- --- --- ---
		// END - PLACE ORDER
		// --- --- --- --- --- ---
		
		SavePurchaseOrder(purchaseOrder);
		
		DisplayInventory(vendorIDWithActiveOrder);
		DisplayOrders();
	}
	#region catch exception
	catch (ArgumentNullException ex)
	{
		GetInnerException(ex).Message.Dump();
	}
	catch (ArgumentException ex)
	{

		GetInnerException(ex).Message.Dump();
	}
	catch (AggregateException ex)
	{
		//having collected a number of errors
		//	each error should be dumped to a separate line
		foreach (var error in ex.InnerExceptions)
		{
			error.Message.Dump();
		}
	}
	catch (Exception ex)
	{
		GetInnerException(ex).Message.Dump();
	}
	#endregion
}

#region TransactionServiceMethods
public void SavePurchaseOrder(PurchaseOrderView purchaseOrder)
{
	List<Exception> errorList = new List<Exception>();
	PurchaseOrders newPurchaseOrder = new PurchaseOrders();

	if (purchaseOrder == null || purchaseOrder.PurchaseOrderDetails.Count() == 0)
	{
		throw new ArgumentNullException("No purchase order or purchase order details submittted.");
	}

	if (purchaseOrder.VendorID == 0)
	{
		errorList.Add(new Exception($"Vendor ID is missing."));
	}

	Vendors vendor = Vendors
						.Where(v => v.VendorID == purchaseOrder.VendorID)
						.FirstOrDefault();
	if (vendor == null)
	{
		errorList.Add(new Exception($"Vendor ID {purchaseOrder.VendorID} does not exist in the database."));
	}

	if (purchaseOrder.EmployeeID == 0)
	{
		errorList.Add(new Exception($"Employee ID is missing."));
	}

	Employees employee = Employees
							.Where(v => v.EmployeeID == purchaseOrder.EmployeeID)
							.FirstOrDefault();
	if (employee == null)
	{
		errorList.Add(new Exception($"Employee ID {purchaseOrder.EmployeeID} does not exist in the database."));
	}

	if (errorList.Count == 0)
	{
		newPurchaseOrder.PurchaseOrderNumber = purchaseOrder.PurchaseOrderNumber;
		newPurchaseOrder.OrderDate = purchaseOrder.OrderDate;
		newPurchaseOrder.Closed = false;
		newPurchaseOrder.EmployeeID = purchaseOrder.EmployeeID;
		newPurchaseOrder.VendorID = purchaseOrder.VendorID;
	}

	// Check the purchase order details
	foreach (var purchaseDetail in purchaseOrder.PurchaseOrderDetails)
	{
		if (purchaseDetail.QuantityToOrder <= 0)
		{
			errorList.Add(new Exception($"Part {purchaseDetail.Description} quantity to order must be greater than zero."));
		}

		if (purchaseDetail.PurchasePrice <= 0)
		{
			errorList.Add(new Exception($"Part {purchaseDetail.Description} price must be greater than zero."));
		}

		if (errorList.Count == 0)
		{
			PurchaseOrderDetails newPurchaseOrderDetail = new PurchaseOrderDetails();
			newPurchaseOrderDetail.PartID = purchaseDetail.PartID;
			newPurchaseOrderDetail.Quantity = purchaseDetail.QuantityToOrder;
			newPurchaseOrderDetail.PurchasePrice = purchaseDetail.PurchasePrice;
			newPurchaseOrder.PurchaseOrderDetails.Add(newPurchaseOrderDetail);
		}
	}

	newPurchaseOrder.SubTotal = newPurchaseOrder.PurchaseOrderDetails
									.Sum(pod => pod.Quantity * pod.PurchasePrice);
	newPurchaseOrder.TaxAmount = newPurchaseOrder.SubTotal * 0.05m;
	PurchaseOrders.Add(newPurchaseOrder);
	
	// When order date is not null, it is a place order
	if (newPurchaseOrder.OrderDate != null)
	{
		// Update the quantity on order for each parts
		foreach (var purchaseOrderDetail in newPurchaseOrder.PurchaseOrderDetails)
		{
			Parts part = Parts
							.Where(p => p.PartID == purchaseOrderDetail.PartID)
							.Select(p => p).FirstOrDefault();
			part.QuantityOnOrder = part.QuantityOnOrder + purchaseOrderDetail.Quantity;
			Parts.Update(part);
		}
	}

	if (errorList.Count > 0)
	{
		throw new AggregateException("Unable to process the order. Check concerns", errorList.OrderBy(x => x.Message).ToList());
	}
	else
	{
		SaveChanges();
	}
}

#endregion

#region QueryServiceMethods
public PurchaseOrderView GetPurchaseOrder(int vendorID)
{
	if (vendorID == 0)
	{
		throw new ArgumentNullException("Vendor ID was not provided.");
	}

	if (!Vendors.Any(v => v.VendorID == vendorID))
	{
		throw new ArgumentException("Vendor ID does not exist.");
	}

	// Get the current active order for the specified vendor
	PurchaseOrderView purchaseOrder = PurchaseOrders
		.Where(po => po.VendorID == vendorID
			&& po.OrderDate == null)
		.Select(po => new PurchaseOrderView
		{
			PurchaseOrderID = po.PurchaseOrderID,
			PurchaseOrderNumber = po.PurchaseOrderNumber,
			EmployeeID = po.EmployeeID,
			VendorID = po.VendorID,
			OrderDate = po.OrderDate.Value,
			SubTotal = po.SubTotal,
			TaxAmount = po.TaxAmount,
			Notes = po.Notes,
			PurchaseOrderDetails = PurchaseOrderDetails
				.Where(pod => pod.PurchaseOrderID == po.PurchaseOrderID)
				.OrderBy(pod => pod.PartID)
				.Select(pod => new PurchaseOrderDetailView
				{
					PurchaseOrderDetailID = pod.PurchaseOrderDetailID,
					PartID = pod.PartID,
					Description = pod.Part.Description,
					QuantityOnHand = pod.Part.QuantityOnHand,
					ReorderLevel = pod.Part.ReorderLevel,
					QuantityOnOrder = pod.Part.QuantityOnOrder,
					QuantityToOrder = pod.Quantity,
					PurchasePrice = pod.PurchasePrice
				}).ToList()
		}).FirstOrDefault();

	// If no current active order, get a suggested order
	if (purchaseOrder == null)
	{
		purchaseOrder = new PurchaseOrderView();
		purchaseOrder.VendorID = vendorID;
		purchaseOrder.PurchaseOrderDetails = Parts
			.Where(p => p.VendorID == vendorID
				&& p.ReorderLevel > (p.QuantityOnHand + p.QuantityOnOrder))
			.Select(p => new PurchaseOrderDetailView
			{
				PartID = p.PartID,
				Description = p.Description,
				QuantityOnHand = p.QuantityOnHand,
				ReorderLevel = p.ReorderLevel,
				QuantityOnOrder = p.QuantityOnOrder,
				QuantityToOrder = p.ReorderLevel - (p.QuantityOnHand + p.QuantityOnOrder),
				PurchasePrice = p.PurchasePrice
			}).ToList();
	}

	return purchaseOrder;
}

public List<VendorInventoryView> GetVendorInventory(int vendorID)
{
	if (vendorID == 0)
	{
		throw new ArgumentNullException("Vendor ID was not provided.");
	}

	if (!Vendors.Any(v => v.VendorID == vendorID))
	{
		throw new ArgumentException("Vendor ID does not exist.");
	}

	// Get the active order part id(s)
	List<int> purchaseOrderIDs = PurchaseOrderDetails
		.Where(pod => pod.PurchaseOrder.VendorID == vendorID
			&& pod.PurchaseOrder.OrderDate == null)
		.Select(pod => pod.PartID).ToList();

	List<VendorInventoryView> vendorInventoryList;

	// When there is an active order, get only the parts that is not in the purchase order details
	if (purchaseOrderIDs.Count() != 0)
	{
		vendorInventoryList = Parts
			.Where(p => p.VendorID == vendorID
				&& purchaseOrderIDs.All(poId => poId != p.PartID))
			.Select(p => new VendorInventoryView
			{
				PartID = p.PartID,
				Description = p.Description,
				QuantityOnHand = p.QuantityOnHand,
				ReorderLevel = p.ReorderLevel,
				QuantityOnOrder = p.QuantityOnOrder,
				Buffer = (p.QuantityOnHand + p.QuantityOnOrder) - p.ReorderLevel,
				PurchasePrice = p.PurchasePrice
			}).ToList();
	}
	// When there is no active order, get only the parts that is not in the suggested order details 
	else
	{
		vendorInventoryList = Parts
			.Where(p => p.VendorID == vendorID
				&& p.ReorderLevel <= (p.QuantityOnHand + p.QuantityOnOrder))
			.Select(p => new VendorInventoryView
			{
				PartID = p.PartID,
				Description = p.Description,
				QuantityOnHand = p.QuantityOnHand,
				ReorderLevel = p.ReorderLevel,
				QuantityOnOrder = p.QuantityOnOrder,
				Buffer = (p.QuantityOnHand + p.QuantityOnOrder) - p.ReorderLevel,
				PurchasePrice = p.PurchasePrice
			}).ToList();
	}

	return vendorInventoryList;
}

public List<VendorView> GetVendors()
{
	return Vendors
		.OrderBy(v => v.VendorName)
		.Select(v => new VendorView
		{
			VendorID = v.VendorID,
			VendorName = v.VendorName,
			Phone = v.Phone,
			City = v.City
		}).ToList();
}
#endregion

#region Utility
private Exception GetInnerException(Exception ex)
{
	while (ex.InnerException != null)
		ex = ex.InnerException;
	return ex;
}
#endregion

#region DataSeeding
public void FetchDataTest(int vendorIDWithOutActiveOrder, int vendorIDWithActiveOrder)
{
	Console.WriteLine("Getting the list of vendors...");
	GetVendors().Dump();

	Console.WriteLine("Getting a purchase order with NO CURRENT ACTIVE ORDER...");
	Console.WriteLine("Purchase order with the SUGGESTED purchase order details");
	GetPurchaseOrder(vendorIDWithOutActiveOrder).Dump();
	Console.WriteLine("Parts that is not in the SUGGESTED purchase order details");
	GetVendorInventory(vendorIDWithOutActiveOrder).Dump();
	
	Console.WriteLine("Getting a purchase order with CURRENT ACTIVE ORDER...");
	Console.WriteLine("IMPORTANT: If OrderDate displays not NULL value, please CHANGE the PurchaseOrders schema and REMOVE OrderDate 'Default Value or Binding' property to blank.");
	Console.WriteLine("Purchase order with the purchase order details");
	GetPurchaseOrder(vendorIDWithActiveOrder).Dump();
	Console.WriteLine("Parts that is not in the purchase order details");
	GetVendorInventory(vendorIDWithActiveOrder).Dump();
}

public PurchaseOrderView CreateGoodPurchaseOrder(int vendorID, bool displayData=true)
{
	PurchaseOrderView purchaseOrder = new PurchaseOrderView();

	// Check if there is no current active order for the specific vendor
	int purchaseOrderID = PurchaseOrders
							.Where(po => po.OrderDate == null && po.VendorID == vendorID)
							.Select(po => po.PurchaseOrderID)
							.FirstOrDefault();

	if (purchaseOrderID == 0)
	{
		// These array are test data
		int[] qtyToOrder = { 49, 4, 4 };
		decimal[] price = { 10.00m, 10.00m, 40.00m };

		purchaseOrder.PurchaseOrderNumber = PurchaseOrders.Max(po => po.PurchaseOrderNumber) + 1;
		purchaseOrder.EmployeeID = 7;
		purchaseOrder.VendorID = vendorID;

		// Create purchase order details
		List<Parts> partList = Parts
								.Take(3)
								.Where(p => p.VendorID == vendorID)
								.OrderByDescending(p => p.PartID)
								.Select(p => p).ToList();

		if (partList != null)
		{
			// Create different scenario for purchase order details				
			int index = 0;
			purchaseOrder.PurchaseOrderDetails = new List<PurchaseOrderDetailView>();

			foreach (var part in partList)
			{
				PurchaseOrderDetailView purchaseOrderDetailView = new PurchaseOrderDetailView();
				purchaseOrderDetailView.PartID = part.PartID;
				purchaseOrderDetailView.Description = part.Description;
				purchaseOrderDetailView.QuantityOnHand = part.QuantityOnHand;
				purchaseOrderDetailView.ReorderLevel = part.ReorderLevel;
				purchaseOrderDetailView.QuantityOnOrder = part.QuantityOnOrder;
				purchaseOrderDetailView.QuantityToOrder = qtyToOrder[index];
				purchaseOrderDetailView.PurchasePrice = price[index];
				purchaseOrder.PurchaseOrderDetails.Add(purchaseOrderDetailView);
				index++;
			}
		}

		purchaseOrder.SubTotal = purchaseOrder.PurchaseOrderDetails.Sum(pod => pod.QuantityToOrder * pod.PurchasePrice);
		purchaseOrder.TaxAmount = purchaseOrder.SubTotal * 0.05m;
	}
	
	if (displayData)
	{
		purchaseOrder.Dump();
	}
	
	return purchaseOrder;
}

public PurchaseOrderView CreateBadPurchaseOrder(int vendorID)
{
	PurchaseOrderView purchaseOrder = new PurchaseOrderView();
	purchaseOrder.PurchaseOrderNumber = PurchaseOrders.Max(po => po.PurchaseOrderNumber) + 1;
	purchaseOrder.EmployeeID = 999;
	purchaseOrder.VendorID = 999;

	// Create purchase order details
	List<Parts> partList = Parts
							.Take(3)
							.Where(p => p.VendorID == vendorID)
							.OrderByDescending(p => p.PartID)
							.Select(p => p).ToList();

	// Create different scenario for purchase order details				
	int index = 0;
	purchaseOrder.PurchaseOrderDetails = new List<PurchaseOrderDetailView>();

	foreach (var part in partList)
	{
		PurchaseOrderDetailView purchaseOrderDetailView = new PurchaseOrderDetailView();
		purchaseOrderDetailView.PartID = part.PartID;
		purchaseOrderDetailView.Description = part.Description;
		purchaseOrderDetailView.QuantityOnHand = part.QuantityOnHand;
		purchaseOrderDetailView.ReorderLevel = part.ReorderLevel;
		purchaseOrderDetailView.QuantityOnOrder = part.QuantityOnOrder;

		// Scenario 0: Quantity To Order is zero and Price is negative
		if (index == 0)
		{
			purchaseOrderDetailView.QuantityToOrder = 0;
			purchaseOrderDetailView.PurchasePrice = -13.00m;
		}
		// Scenario 1: Quantity To Order is negative and Price is zero
		else if (index == 1)
		{
			purchaseOrderDetailView.QuantityToOrder = -7;
			purchaseOrderDetailView.PurchasePrice = 0.00m;
		}
		// Scenario 2: Good Data
		else
		{
			purchaseOrderDetailView.QuantityToOrder = 4;
			purchaseOrderDetailView.PurchasePrice = part.PurchasePrice;
		}

		purchaseOrder.PurchaseOrderDetails.Add(purchaseOrderDetailView);
		index++;
	}

	purchaseOrder.SubTotal = purchaseOrder.PurchaseOrderDetails.Sum(pod => pod.QuantityToOrder * pod.PurchasePrice);
	purchaseOrder.TaxAmount = purchaseOrder.SubTotal * 0.05m;
	purchaseOrder.Dump();

	return purchaseOrder;
}

public void DisplayOrders()
{
	Console.WriteLine("Displaying latest purchase orders...");
	PurchaseOrders
		.OrderByDescending(po => po.PurchaseOrderID)
		.Take(5)
		.Select(po => new {
			PurchaseOrderID = po.PurchaseOrderID,
			VendorName = po.Vendor.VendorName,
			Phone = po.Vendor.Phone,
			City = po.Vendor.City,
			PurchaseOrderNumber = po.PurchaseOrderNumber,
			OrderDate = po.OrderDate,
			Subtotal = po.SubTotal,
			GST = po.TaxAmount,
			Total = po.SubTotal + po.TaxAmount
		}).ToList().Dump();
		
	Console.WriteLine("Displaying latest purchase order details...");
	PurchaseOrderDetails
		.OrderByDescending(pod => pod.PurchaseOrderID)
		.ThenBy(pod => pod.PartID)
		.Take(15)
		.Select(pod => new {
			PurchaseOrderID = pod.PurchaseOrderID,
			PartID = pod.PartID,
			Description = pod.Part.Description,
			QOH = pod.Part.QuantityOnHand,
			ROL = pod.Part.ReorderLevel,
			QOO = pod.Part.QuantityOnOrder,
			QTO = pod.Quantity,
			Price = pod.PurchasePrice
		}).ToList().Dump();
}

public void DisplayInventory(int vendorID)
{
	Console.WriteLine($"Displaying current inventory for this vendor ID {vendorID}...");
	Parts
		.Where(p => p.VendorID == vendorID)
		.OrderBy(p => p.PartID)
		.Select(p => new {
			p.PartID,
			p.Description,
			p.PurchasePrice,
			p.QuantityOnHand,
			p.ReorderLevel,
			p.QuantityOnOrder
		}).Dump();
}

public void ResetData(bool useForFetch=true)
{
	Console.WriteLine("Resetting data...");
	
	// Create data for suggested purhcase order
	// Create vendor if it does not exists
	string vendorName = "Suggested Ltd";
	int vendorID = CreateVendor(vendorName);
	// Create parts if it does not exists
	CreateParts(vendorID);
	DeleteOrders(vendorID);
	
	// Create data for active purchase order
	// Create vendor if it does not exists
	vendorName = "Current Active Ltd";
	vendorID = CreateVendor(vendorName);
	// Create parts if it does not exists
	DeleteOrders(vendorID);
	DeleteParts(vendorID);
	CreateParts(vendorID);
	
	if (useForFetch)
	{
		// Create data for vendor with active order
		PurchaseOrderView purchaseOrder = CreateGoodPurchaseOrder(vendorID);
		SavePurchaseOrder(purchaseOrder);
	}
}

public int CreateVendor(string vendorName)
{
	// Create Vendor
	if (!Vendors.Any(v => v.VendorName == vendorName))
	{
		Vendors referenceVendor = Vendors.Where(v => v.VendorID == 2).FirstOrDefault();
		Vendors newVendor = new Vendors();
		newVendor.VendorName = vendorName;
		newVendor.Phone = referenceVendor.Phone;
		newVendor.Address = referenceVendor.Address;
		newVendor.City = referenceVendor.City;
		newVendor.PostalCode = referenceVendor.PostalCode;
		Vendors.Add(newVendor);
		SaveChanges();
	}

	return Vendors
			.Where(v => v.VendorName == vendorName)
			.Select(v => v.VendorID).FirstOrDefault();
}

public void DeleteParts(int vendorID)
{
	List<Parts> parts = Parts.Where(p => p.VendorID == vendorID)
							.Select(p => p).ToList();

	if (parts != null)
	{
		foreach(var part in parts)
		{
			Parts partToDelete = Parts.Where(p => p.PartID == part.PartID)
									.Select(p => p).FirstOrDefault();
			Parts.Remove(partToDelete);
		}
	}

	SaveChanges();
}

public void CreateParts(int vendorID)
{
	Parts part = null;
	Parts newPart = null;
	int[] referencePartIDList = {102, 103, 104, 105, 107, 109};
	
	if (!Parts.Any(v => v.VendorID == vendorID))
	{
		foreach (int referencePartID in referencePartIDList)
		{
			part = Parts
					.Where(p => p.PartID == referencePartID)
					.Select(p => p).FirstOrDefault();
			
			if (part != null)
			{
				newPart = new Parts();
				newPart.Description = part.Description;
				newPart.PurchasePrice = part.PurchasePrice;
				newPart.SellingPrice = part.SellingPrice;
				newPart.ReorderLevel = part.ReorderLevel;
				newPart.CategoryID = part.CategoryID;
				newPart.Refundable = part.Refundable;
				newPart.Discontinued = part.Discontinued;
				newPart.VendorID = vendorID;

				if (referencePartID == 102)
				{
					newPart.QuantityOnHand = 5;
					newPart.QuantityOnOrder = 2;
				}
				else if (referencePartID == 103)
				{
					newPart.QuantityOnHand = 24;
					newPart.QuantityOnOrder = 0;
				}
				else if (referencePartID == 104)
				{
					newPart.QuantityOnHand = 5;
					newPart.QuantityOnOrder = 5;
				}
				else if (referencePartID == 105)
				{
					newPart.QuantityOnHand = 6;
					newPart.QuantityOnOrder = 0;
				}
				else if (referencePartID == 107)
				{
					newPart.QuantityOnHand = 11;
					newPart.QuantityOnOrder = 35;
				}
				else if (referencePartID == 109)
				{
					newPart.QuantityOnHand = 11;
					newPart.QuantityOnOrder = 49;
				}
				else
				{
					newPart.QuantityOnHand = 200;
					newPart.QuantityOnOrder = 0;
				}

				Parts.Add(newPart);
			}
		}
	}

	SaveChanges();
}

public void DeleteOrders(int vendorID)
{
	List<int> purchaseOrderIDList = null;
	PurchaseOrders purchaseOrderToRemove = null;
	
	List<int> purchaseOrderDetailIDList = null;
	PurchaseOrderDetails purchaseOrderDetailToRemove = null;
	
	purchaseOrderDetailIDList = PurchaseOrderDetails
									.Where(pod => pod.PurchaseOrder.VendorID == vendorID)
									.Select(pod => pod.PurchaseOrderDetailID).ToList();
	
	if (purchaseOrderDetailIDList.Count() != 0)
	{
		foreach (var id in purchaseOrderDetailIDList)
		{
			purchaseOrderDetailToRemove = PurchaseOrderDetails
											.Where(pod => pod.PurchaseOrderDetailID == id)
											.FirstOrDefault();
			if (purchaseOrderDetailToRemove != null)
			{
				PurchaseOrderDetails.Remove(purchaseOrderDetailToRemove);
			}
		}
	}

	purchaseOrderIDList = PurchaseOrders
							.Where(po => po.VendorID == vendorID)
							.Select(po => po.PurchaseOrderID).ToList();
							
	if (purchaseOrderIDList.Count() != 0)
	{
		foreach (var id in purchaseOrderIDList)
		{
			purchaseOrderToRemove = PurchaseOrders
										.Where(po => po.PurchaseOrderID == id)
										.FirstOrDefault();
			if (purchaseOrderToRemove != null)
			{
				PurchaseOrders.Remove(purchaseOrderToRemove);
			}
		}
		
		SaveChanges();
	}
}
#endregion