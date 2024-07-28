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
		
		JobView jobRequest = null;
		List <JobDetailView> jobDetailRequest = null;
		
		DisplayJobs();
		
		jobRequest = CreateGoodJob();
		jobDetailRequest = CreateGoodJobDetail();
		
		//jobRequest = CreateBadJob();
		//jobDetailRequest = CreateBadJobDetail();
		
		RegisterJob(jobRequest, jobDetailRequest);
		
		DisplayJobs();
		
		//ResetData();

	}
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
		foreach (var error in ex.InnerExceptions)
		{
			error.Message.Dump();
		}
	}
	catch (Exception ex)
	{
		GetInnerException(ex).Message.Dump();
	}
}

public void RegisterJob (JobView job, List<JobDetailView> jobDetails)
{
	List<Exception> errorList = new List<Exception>();
	Jobs newJobRequest = new Jobs();
	JobDetails newJobDetails =  new JobDetails();
	
	//job.Dump();
	//jobDetails.Dump();
	
	#region New Job Validation
	
	if (job == null)
	{
		errorList.Add(new ArgumentNullException("No job requested"));
	}

	if (job.EmployeeId == 0)
	{
		errorList.Add(new Exception ("No employee Id found"));
	}
	
	Console.WriteLine(job.EmployeeId);
	if (!Employees.Any(j => j.EmployeeID == job.EmployeeId))
	{
		errorList.Add(new Exception ("Employee ID not found in database"));
	}
	
	Jobs employeeId = Jobs
						.Where (j => j.EmployeeID == job.EmployeeId)
						.Select(j => j).FirstOrDefault();
	
	if (job.ShopRate == 0)
	{
		errorList.Add(new Exception ("No Shop Rate identified"));
	}
	
	if (string.IsNullOrWhiteSpace(job.VehicleIdentificationNumber))
	{
		errorList.Add(new Exception ("VIN not provided"));
	}
	
	if (!Jobs.Any(j => j.VehicleIdentification == job.VehicleIdentificationNumber))
	{
		errorList.Add(new Exception ("VIN not found in database"));
	}
	
	Jobs vin = Jobs
				.Where (j => j.VehicleIdentification == job.VehicleIdentificationNumber)
				.Select (j => j).FirstOrDefault();
				
	if (errorList.Count == 0)
	{
		newJobRequest.ShopRate = 65.50m;
		newJobRequest.EmployeeID = job.EmployeeId;
		newJobRequest.VehicleIdentification = vin.VehicleIdentification;
		newJobRequest.JobDateIn = DateTime.Now;	
	}
	
	#endregion
	 
	#region Job Detail Validation
	if (jobDetails.Count < 0)
	{
		throw new ArgumentNullException ("No job details provided");
	}
	
	foreach (var jobDetail in jobDetails)
	{
	
		if (string.IsNullOrWhiteSpace(jobDetail.Description))
		{
			errorList.Add(new Exception ($"Service description is needed"));
		}
			
		JobDetails description = JobDetails
									.Where (jb => jb.Description == jobDetail.Description)
									.Select (jb => jb).FirstOrDefault();
		
		if (jobDetail.JobHour <= 0)
		{
			errorList.Add(new Exception($"Hours for job detail {jobDetail.Description} must be a positive value"));
		}
		JobDetails jobHours = JobDetails
								.Where (jb => jb.JobHours == jobDetail.JobHour)
								.Select (jb => jb).FirstOrDefault();
		
		if(!Coupons.Any(c => c.CouponID == jobDetail.CouponId))
		{
			errorList.Add(new Exception("Coupon ID not found"));
		}
		Coupons coupon = Coupons
							.Where(c => c.CouponID == jobDetail.CouponId)
							.Select(x => x).FirstOrDefault();
								
		if (errorList.Count == 0)
		{
		
		
		newJobDetails.Description = jobDetail.Description;
		newJobDetails.JobHours = jobDetail.JobHour;
		newJobDetails.Comments = jobDetail.Comment;
		newJobDetails.CouponID = jobDetail.CouponId;
		//newJobDetails.Coupon.CouponDiscount = jobDetail.DiscountPercent;

		newJobRequest.JobDetails.Add(newJobDetails);

		}
	}
	#endregion
	
	#region Total Amount Update
	
	//? Total is estimated time and labour cost
	
	decimal SubTotal = newJobRequest.ShopRate * newJobDetails.JobHours;
	decimal TaxAmount = job.SubTotal * (5/100);
	Jobs.Add(newJobRequest);
	#endregion
	
	if(errorList.Count() > 0)
	{
		throw new AggregateException ("Unable to register a new job", errorList);
	}
	else
	{
		SaveChanges();
	}
}

public void AddStandardService(string standardService)
{
	if(string.IsNullOrWhiteSpace(standardService))
	{
		throw new ArgumentNullException ("Standard Service not provided");
	}
	
	StandardJobs
		.Where (x => x.Description == standardService)
		.Select (x => new {
				Description = x.Description,
				StandardHours = x.StandardHours
		}).FirstOrDefault();
}


public void AddService (string service, decimal hours)
{
	if(StandardJobs.Any(x => x.Description != service))
	{
		StandardJobs newStandardJob = new StandardJobs();
		
		newStandardJob.Description = service;
		newStandardJob.StandardHours = hours;
		StandardJobs.Add(newStandardJob);
	}
	
	StandardJobs
		.Where(x => x.Description == service && x.StandardHours == hours)
		.Select(x => new 
		{
			x.Description,
			x.StandardHours,
		}).FirstOrDefault();
}


private Exception GetInnerException(Exception ex)
{
	while (ex.InnerException != null)
		ex = ex.InnerException;
	return ex;
}

public JobView CreateGoodJob()
{
	Console.WriteLine("Creating Good Data...");
	JobView jobRequest = new JobView();
	jobRequest.ShopRate = 65.50m;
	jobRequest.VehicleIdentificationNumber = "123FT678Y9801";
	jobRequest.EmployeeId = 1;
	
	jobRequest.Dump();
	return jobRequest; 
}

public List<JobDetailView> CreateGoodJobDetail()
{
	Console.WriteLine("Creating Good Data for JobDetails...");
	JobDetailView jobDetailRequest = new JobDetailView();
	jobDetailRequest.Description = "Oil Change";
	jobDetailRequest.JobHour = 0.08m;
	jobDetailRequest.Comment = "First Oil Change";
	jobDetailRequest.CouponId = 66;
	List<JobDetailView> newJobDetailList = new List <JobDetailView>();
	
	newJobDetailList.Add(jobDetailRequest);
	
	newJobDetailList.Dump();
	return newJobDetailList;
}


public JobView CreateBadJob()
{

	Console.WriteLine("Creating Bad Data...");
	JobView jobRequest = new JobView();
	jobRequest.ShopRate = 0;
	jobRequest.VehicleIdentificationNumber = "123YT677Y9801";
	jobRequest.EmployeeId = 15;
	
	jobRequest.Dump();
	return jobRequest;
}

public List<JobDetailView> CreateBadJobDetail()
{
	Console.WriteLine("Creating Bad Data for job Details...");
	JobDetailView jobDetailRequest = new JobDetailView();
	jobDetailRequest.Description = "";
	jobDetailRequest.JobHour = 0.08m;
	jobDetailRequest.Comment = "First Oil Change";
	jobDetailRequest.CouponId = 1;
	List<JobDetailView> newJobDetailList = new List <JobDetailView>();
	
	newJobDetailList.Add(jobDetailRequest);
	
	newJobDetailList.Dump();
	return newJobDetailList;
}

public void ResetData()
{

	foreach (var job in Jobs)
	{
	
		if (job.ShopRate != 0)
		{
			job.ShopRate = 0;
		}
		if (!string.IsNullOrWhiteSpace(job.VehicleIdentification) || string.IsNullOrWhiteSpace(job.VehicleIdentification))
		{
			job.VehicleIdentification = string.Empty;
		}
		if (job.EmployeeID !=0)
		{
			job.EmployeeID = 0;
		}
	}
	
	foreach (var jobDetail in JobDetails)
	{
		if (!string.IsNullOrWhiteSpace(jobDetail.Description) || string.IsNullOrWhiteSpace(jobDetail.Description))
		{
			jobDetail.Description = string.Empty;
		}
		if (jobDetail.JobHours != 0)
		{
			jobDetail.JobHours = 0;
		}
		if (!string.IsNullOrWhiteSpace(jobDetail.Comments) || string.IsNullOrWhiteSpace(jobDetail.Comments))
		{
			jobDetail.Comments = string.Empty;
		}
		if (jobDetail.CouponID != 0)
		{
			jobDetail.CouponID = 0;
		}
		
	}
	SaveChanges();
}

public void DisplayJobs()
{

	Console.WriteLine("Displayig orders:");
	Jobs
		.Select(x => new {
		
			JobID = x.JobID,
			JobDate = x.JobDateIn,
			EmployeeID = x.EmployeeID,
			ShopRate = x.ShopRate,
			VIN = x.VehicleIdentification
		}).ToList().Dump();
		
	JobDetails
		.Select(x => new {
		
			Description = x.Description,
			JobHours = x.JobHours,
			Comments = x.Comments,
			CouponDiscount = Coupons
								.Select( x => x.CouponDiscount > 0 ? x.CouponDiscount : 0).First(),
			//Subtotal = (x.JobHours * x.Job.ShopRate) - x.Coupon.CouponDiscount,
			//Tax = ((x.JobHours * x.Job.ShopRate) - x.Coupon.CouponDiscount) *0.05m
		}).ToList().Dump();
		
}

