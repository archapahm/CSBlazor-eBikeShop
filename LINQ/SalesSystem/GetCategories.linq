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
		List<CategoryView> allCategories = Sale_GetCategories();
		allCategories.Dump();
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

public List<CategoryView> Sale_GetCategories()
{
	return Categories.Select(x=> new CategoryView
		{
			CategoryID = x.CategoryID,
			Description = x.Description
		}).ToList();
}