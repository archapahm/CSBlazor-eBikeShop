<Query Kind="Program">
  <Connection>
    <ID>cdfb91fd-2f0e-477d-962f-7efc3a9cb963</ID>
    <NamingServiceVersion>2</NamingServiceVersion>
    <Persist>true</Persist>
    <Driver Assembly="(internal)" PublicKeyToken="no-strong-name">LINQPad.Drivers.EFCore.DynamicDriver</Driver>
    <Server>Com_puter</Server>
    <AttachFileName>C:\Users\archa\Downloads\eBike_DMIT2018.dacpac</AttachFileName>
    <Database>eBike_DMIT2018</Database>
    <DisplayName>eBikeEntity</DisplayName>
    <DriverData>
      <EncryptSqlTraffic>True</EncryptSqlTraffic>
      <PreserveNumeric1>True</PreserveNumeric1>
      <EFProvider>Microsoft.EntityFrameworkCore.SqlServer</EFProvider>
      <EFVersion>6.0.10</EFVersion>
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
		string goodCoupon = "OilChg01";
		
		Console.WriteLine("Coupon exists in the database");
		GetCoupon(goodCoupon).Dump();
		
		string badCoupon = "OilCg1";
		
		Console.WriteLine("Coupon not found in the database. Coupon discount of zero");
		GetCoupon(badCoupon).Dump();
			
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

private Exception GetInnerException (Exception ex)
{
	while (ex.InnerException != null)
		ex = ex.InnerException;
	return ex;
}

#endregion

public int GetCoupon (string coupons)
{
	if (string.IsNullOrWhiteSpace(coupons))
	{
		throw new ArgumentNullException ("No cupon added");
	}
	if (!Coupons.Any(c => c.CouponIDValue == coupons))
	{
		Coupons invalidCoupon = new Coupons();
		invalidCoupon.CouponDiscount = 0;
	}

	return 	Coupons
				.Where(x => x.CouponIDValue == coupons)
				.Select(x => x.CouponDiscount)
				.FirstOrDefault();
	
}

// You can define other methods, fields, classes and namespaces here