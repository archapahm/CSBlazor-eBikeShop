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
		int customerHasVehicle = 5;
		
		List<CustomerVehicleView> vehicleView = Service_GetCustomerVehicle (customerHasVehicle);
		
		int noVehicleCustomer = 6;
	
		List<CustomerVehicleView> noVehicleView = Service_GetCustomerVehicle (noVehicleCustomer);
		
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

public List<CustomerVehicleView> Service_GetCustomerVehicle (int customerId)
{

	//Business Rule
	// rule: customer must have a vehicle
	// rule: customerId cannot be null
	
	if (customerId == 0)
	{
		throw new ArgumentNullException("Customer not selected");
	}

	
	List<CustomerVehicleView> customerVehicle =  CustomerVehicles
													.Where(x => x.CustomerID == customerId)
													.Select(x => new CustomerVehicleView
													{
														MakeModel = x.Make + x.Model,
														VehicleIdentificationNumber = x.VehicleIdentification
													}).ToList().Dump();
													
	if (customerVehicle.Count() == 0)
	{
		throw new ArgumentException ("No vehicle found for customer");
	}
	
	return customerVehicle;
}

// You can define other methods, fields, classes and namespaces here