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
		Sales.Dump();
		SaleDetailView goodData = new SaleDetailView();
		goodData.PartID = 101;
		goodData.Description = "Forged pistons";
		goodData.Quantity = 1;
		goodData.SellingPrice = (decimal)50.00;
		goodData.Total = (decimal)50.00;
		List<SaleDetailView> saleDetails = new List<SaleDetailView>();
		saleDetails.Add(goodData);
		SaleView goodSale = new SaleView();
		goodSale.CouponId = 71;
		goodSale.DiscountPercent = 5;
		goodSale.EmployeeId = 1;
		goodSale.PaymentType = "M";
		goodSale.SubTotal = goodData.Total;
		goodSale.TaxAmount = goodSale.SubTotal * (decimal)0.05;
		Checkout(goodSale, saleDetails).Dump();
		Sales.Dump();
		
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

#endregion

public int Checkout(SaleView sale, List<SaleDetailView> saleDetails)
{
	List<Exception> errorlist = new List<Exception>();
	Sales newSale = new Sales();
	SaleDetails newDetailEntry = new SaleDetails();
	List<SaleDetails> newDetails = new List<SaleDetails>();
	if(saleDetails.Count() <= 0){
		errorlist.Add(new ArgumentNullException("No sale details were submitted"));
	}
	int qoh;
	foreach (var i in saleDetails)
	{
		bool partExists = Parts.Where(x=>x.PartID == i.PartID).Any();
		if(partExists)
		{
			int currentStock = Parts.Where(x => x.PartID == i.PartID).Select(x => x.QuantityOnHand).First();
			if (i.Quantity < 0)
			{
				errorlist.Add(new Exception($"Quantity for sale item {i} must be a positive value"));
			}
			else if (currentStock < i.Quantity)
			{
				errorlist.Add(new Exception($"{i} is out of stock"));
			}
			else
			{
				newDetailEntry.PartID = i.PartID;
				newDetailEntry.Part = Parts.Where(x => x.PartID == i.PartID).First();
				newDetailEntry.Quantity = i.Quantity;
				newDetailEntry.SellingPrice = i.SellingPrice;
				newDetails.Add(newDetailEntry);
				qoh = Parts.Where(x => x.PartID == i.PartID).Select(x => x.QuantityOnHand).First();
				qoh -= i.Quantity;
			}
		}
		else
		{
			errorlist.Add(new Exception($"Part with ID of {i.PartID} does not exist"));
		}
	}
	bool couponExists = Coupons.Where(x=>x.CouponID == sale.CouponId).Any();
	if(couponExists || sale.CouponId == 0)
	{
		if (sale.CouponId != 0)
		{
			newSale.SubTotal = sale.SubTotal - (sale.SubTotal * ((decimal)sale.DiscountPercent/100));
			newSale.TaxAmount = sale.SubTotal * (decimal)0.05;
			newSale.CouponID = sale.CouponId;
		}
		else
		{
			newSale.TaxAmount = sale.TaxAmount;
			newSale.SubTotal = sale.SubTotal;
		}
	}
	else
	{
		errorlist.Add(new Exception($"No coupon with ID {sale.CouponId} exists"));
	}

	newSale.EmployeeID = sale.EmployeeId;	
	newSale.PaymentType = sale.PaymentType;
	newSale.SaleDate = DateTime.Today;
	newSale.SaleDetails = newDetails;
	
	Sales.Add(newSale);
	
	if(errorlist.Count() > 0)
	{
		throw new AggregateException("Error: ", errorlist);
	}
	else
	{
		SaveChanges();
		return newSale.SaleID;
	}
}
