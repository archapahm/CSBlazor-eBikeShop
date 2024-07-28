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
		CouponView testCoupon = GetCoupon("topHat").Dump();
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

public CouponView GetCoupon(string coupon)
{
	bool couponExists = Coupons.Where(x => x.CouponIDValue.Equals(coupon)).Any();
	if(!couponExists)
	{
		CouponView noCoupon = new CouponView();
		noCoupon.CouponDiscount=0;
		noCoupon.CouponId=0;
		return noCoupon;
	}
	else
	{
		return Coupons.Where(x => x.CouponIDValue.Equals(coupon)).Select(x => new CouponView
			{
				CouponDiscount = x.CouponDiscount,
				CouponId = x.CouponID
			}).First();
	}
}