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
		List<string> categories = Service_GetStandardService().Dump();
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

public List<string> Service_GetStandardService()
{
	
	return StandardJobs
			.Select (x => x.Description).ToList();
}


// You can define other methods, fields, classes and namespaces here