<Query Kind="Program">
  <Connection>
    <ID>584d5b5f-ea0a-4f12-a75e-7e20b74cf875</ID>
    <NamingServiceVersion>2</NamingServiceVersion>
    <Persist>true</Persist>
    <Driver Assembly="(internal)" PublicKeyToken="no-strong-name">LINQPad.Drivers.EFCore.DynamicDriver</Driver>
    <Server>HUNTERDESKTOP</Server>
    <Database>eBike_DMIT2018</Database>
    <DisplayName>eBike</DisplayName>
    <DriverData>
      <EncryptSqlTraffic>True</EncryptSqlTraffic>
      <PreserveNumeric1>True</PreserveNumeric1>
      <EFProvider>Microsoft.EntityFrameworkCore.SqlServer</EFProvider>
    </DriverData>
  </Connection>
</Query>

#load ".\ViewModels\*.cs"
using eBike;
void Main()
{
	try
	{
		SaleRefundView newRefund = new SaleRefundView();
		SaleRefundDetailView newRefundDetailEntry = new SaleRefundDetailView();
		List<SaleRefundDetailView> newRefundDetails = new List<SaleRefundDetailView>();
		newRefundDetailEntry.PartID = 101;
		newRefundDetailEntry.Reason = "do not want";
		newRefundDetailEntry.Quantity = 1;
		newRefundDetailEntry.OriginalQuantity = 1;
		newRefundDetails.Add(newRefundDetailEntry);
		newRefund.SaleId = 2024;
		newRefund.EmployeeId = 1;
		newRefund.SubTotal = (decimal)47.50;
		newRefund.TaxAmount = (decimal)(50 * 0.05);
		SaleRefund_Refund(newRefund, newRefundDetails).Dump();
		
		newRefund.SaleId = 20000;
		SaleRefund_Refund(newRefund, newRefundDetails).Dump();
	}
	#region catch exceptions
	catch (AggregateException ex)
	{
		foreach (var error in ex.InnerExceptions)
		{
			error.Message.Dump();
		}
	}
	catch (ArgumentNullException ex)
	{
		GetInnerException(ex).Message.Dump();
	}
	catch (Exception ex)
	{
		GetInnerException(ex).Message.Dump();
	}
	#endregion
}

#region Methods

private Exception GetInnerException(Exception ex)
{
	while (ex.InnerException != null)
		ex = ex.InnerException;
	return ex;
}

public SaleRefundView SaleRefund_Refund(SaleRefundView saleRefund, List<SaleRefundDetailView> saleRefundDetails)
{
	List<Exception> errorlist = new List<Exception>();
	SaleRefunds newRefund = new SaleRefunds();
	SaleRefundDetails newRefundDetailEntry = new SaleRefundDetails();
	List<SaleRefundDetails> newRefundDetails = new List<SaleRefundDetails>();
	bool saleExists = Sales.Where(x=>x.SaleID == saleRefund.SaleId).Any();
	if (saleExists)
	{
		foreach (var i in saleRefundDetails)
		{
			if (i.Quantity < 0)
			{
				errorlist.Add(new Exception($"Quantity for item {i.Description} must be greater than 0"));
			}
			if (i.Quantity > 0 && i.Reason == "")
			{
				errorlist.Add(new Exception($"There is no reason given for {i.Description}"));
			}
			if (i.Quantity > i.OriginalQuantity)
			{
				errorlist.Add(new Exception($"The quantity of {i.Description} you are trying to return exceeds the number on the original purchase"));
			}
			if (i.Refundable)
			{
				errorlist.Add(new Exception($"{i.Description} is not refundable"));
			}
		}
	}
	else
	{
		errorlist.Add(new Exception($"No sale with ID {saleRefund.SaleId} exists"));
	}

	newRefund.SaleRefundDate = DateTime.Today;
	newRefund.SaleID = saleRefund.SaleId;
	newRefund.SubTotal = saleRefund.SubTotal;
	newRefund.EmployeeID = saleRefund.EmployeeId;
	newRefund.TaxAmount = saleRefund.TaxAmount;

	foreach (var i in SaleRefundDetails)
	{
		newRefundDetailEntry.PartID = i.PartID;
		newRefundDetailEntry.Part = i.Part;
		newRefundDetailEntry.Quantity = i.Quantity;
		newRefundDetailEntry.Reason = i.Reason;
		newRefundDetailEntry.SaleRefund = i.SaleRefund;
		newRefundDetailEntry.SaleRefundDetailID = i.SaleRefundDetailID;
		newRefundDetailEntry.SaleRefundID = i.SaleRefundID;
		newRefundDetailEntry.SellingPrice = i.SellingPrice;
		newRefundDetails.Add(newRefundDetailEntry);
	}
	
	newRefund.SaleRefundDetails = newRefundDetails;
	
	if(errorlist.Count() > 0)
	{
		throw new AggregateException("Error: ", errorlist);
	}
	else
	{
		SaveChanges();
		return saleRefund;
	}
}

#endregion

