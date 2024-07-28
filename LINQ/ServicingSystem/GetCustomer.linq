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
  <Reference Relative="ViewModels\CustomerView.cs">&lt;MyDocuments&gt;\GitHub\2018-jan-2023-a01-project-team2-a01-jan2023\LINQ\ServicingSystem\ViewModels\CustomerView.cs</Reference>
</Query>

#load ".\ViewModels\*.cs"
using eBike;
void Main()
{
	try
	{
		
		string goodLastName = "Jones"; 
		
		Console.WriteLine("Customer exists in the database:");
		List<CustomerView> goodCustomerTable = Service_GetCustomer (goodLastName);
		
		//string badLastName = "Smit";
		//Console.WriteLine("Customer not found in the database:");
		//List <CustomerView> badCustomerTable = Service_GetCustomer (badLastName);
		
		string missingName = " ";
		Console.WriteLine("Last name not provided:");
		List<CustomerView> missingCustomer = Service_GetCustomer (missingName);

		
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

public List<CustomerView> Service_GetCustomer (string lastName)
{
	//Business Rules
	//	rule: customer lastname cannot be empty
	//	rule: customer must exist in the database
	
	if (string.IsNullOrWhiteSpace(lastName))
	{
		throw new ArgumentNullException ("Customer last name is missing");
	}
	
	if (!Customers.Any(x=> x.LastName == lastName))
	{
		throw new ArgumentException ("Name not found in the database");
	}
	
	List<CustomerView> customers = Customers
			.Where (x => x.LastName == lastName)
			.Select (x => new CustomerView
			{
				CustomerID = x.CustomerID,
				Name = x.FirstName + x.LastName,
				Phone = x.ContactPhone,
				Address = x.Address
			}).ToList().Dump();
	
	return customers;
	
}
