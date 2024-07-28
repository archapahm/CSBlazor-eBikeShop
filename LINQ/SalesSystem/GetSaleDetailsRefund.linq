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
		SaleRefund_GetSaleDetailsRefund(1203).Dump();
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

public List<SaleRefundDetailView> SaleRefund_GetSaleDetailsRefund(int saleId)
{
	bool saleExists = Sales.Where(x=>x.SaleID == saleId).Any();
	if (!saleExists)
	{
		throw new Exception($"No sale with ID of {saleId} found");
	}
	else
	{
		return SaleDetails.Where(x => x.SaleID == saleId).Select(x => new SaleRefundDetailView
		{
			OriginalQuantity = x.Quantity,
			PartID = x.PartID,
			Refundable = x.Part.Refundable == "Y" ? true: false,
			Description = x.Part.Description,
			Quantity = 0,
			Reason = SaleRefundDetails.Where(y=>y.SaleRefund.SaleID==x.SaleID).Any() ? SaleRefundDetails.Where(z=>z.SaleRefund.SaleID==x.SaleID).Select(a=>a.Reason).First() : "",
			ReturnQuantity = SaleRefundDetails.Where(y=>y.SaleRefund.SaleID==x.SaleID).Any() ? SaleRefundDetails.Where(z=>z.SaleRefund.SaleID==x.SaleID).Select(a=>a.Quantity).First() : 0,
			SellingPrice = x.SellingPrice
		}).ToList();
	}
}

#endregion

