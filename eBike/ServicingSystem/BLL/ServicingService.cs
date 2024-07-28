using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using ServicingSystem.DAL;
using ServicingSystem.Entities;
using ServicingSystem.ViewModels;


namespace ServicingSystem.BLL
{
	public class ServicingService
	{
		#region Fields
		private readonly ServicingContext _context;
		#endregion

		internal ServicingService( ServicingContext servicingContext)
		{
			_context = servicingContext;
		}

		//GetCustomer
		public async Task<List<CustomerView>> GetCustomer(string lastName)
		{
			//Business Rules
			//	rule: customer lastname cannot be empty
			//	rule: customer must exist in the database

			if (string.IsNullOrWhiteSpace(lastName))
			{
				throw new ArgumentNullException("Customer last name is missing");
			}

			if (!_context.Customers.Any(x => x.LastName == lastName))
			{
				throw new ArgumentException("Name not found in the database");
			}

			return await Task.FromResult(
			_context.Customers
					.Where(x => x.LastName == lastName)
					.Select(x => new CustomerView
					{
						CustomerID = x.CustomerID,
						Name = x.FirstName + x.LastName,
						Phone = x.ContactPhone,
						Address = x.Address
					}).ToList());

		}

		//GetCustomerVehicle
		public async Task<List<CustomerVehicleView>> GetCustomerVehicle(int customerId)
		{

			//Business Rule
			// rule: customer must have a vehicle
			// rule: customerId cannot be null

			if (customerId == 0)
			{
				throw new ArgumentNullException("Customer not selected");
			}

            if (_context.CustomerVehicles.Count() == 0)
            {
                throw new ArgumentException("No vehicle found for customer");
            }

            return await Task.FromResult(_context.CustomerVehicles
															.Where(x => x.CustomerID == customerId)
															.Select(x => new CustomerVehicleView
															{
																MakeModel = x.Make + x.Model,
																VehicleIdentificationNumber = x.VehicleIdentification
															}).ToList());

			

		}

		//GetStandardService
		public List<string> GetStandardService()
		{

			return _context.StandardJobs
					.Select(x => x.Description).ToList();
		}

		//GetCuponDiscount
		public async Task <int> GetCoupon(string coupons)
		{
			
			/*if (string.IsNullOrWhiteSpace(coupons))
			{
				throw new ArgumentNullException("No cupon added");
			}*/
			if (!_context.Coupons.Any(c => c.CouponIDValue == coupons))
			{
				int invalidCoupon = 0;

				return invalidCoupon;
				//Coupons invalidCoupon = newCoupons();
				//invalidCoupon.CouponDiscount = 0;
			}

			return _context.Coupons
						.Where(x => x.CouponIDValue == coupons)
						.Select(x => x.CouponDiscount)
						.First();

		}

		//Job
		public void Job()
		{
			_context.JobDetails
					.Select(x => new
					{
						EmployeeID = x.EmployeeID,
						ShopRate = x.Job.ShopRate,
						VehicleIdentificationNumber = x.Job.VehicleIdentification,
						SubTotal = x.Job.ShopRate * x.JobHours,
						//TaxAmount = (x.Job.ShopRate * x.JobHours) - x.Coupon.CouponDiscount
					}).ToList();

		}

		//JobDetail
		public void JobDetail()
		{

			_context.JobDetails
				.Select(x => new
				{
					Description = x.Description,
					JobHour = x.JobHours,
					Comment = x.Comments,
					CouponID = x.CouponID,
					Discount = x.Coupon.CouponIDValue
				}).ToList();
		}

		//RegisterJob
		public void RegisterJob(JobView job, List<JobDetailView> jobDetails)
		{
			List<Exception> errorList = new List<Exception>();
			
			Job newJobRequest = new Job();
			JobDetail newJobDetails = new JobDetail();

			//job.Dump();
			//jobDetails.Dump();

			#region New Job Validation

			if (job == null)
			{
				errorList.Add(new ArgumentNullException("No job requested"));
			}

			if (job.EmployeeId == 0)
			{
				errorList.Add(new Exception("No employee Id found"));
			}

			Console.WriteLine(job.EmployeeId);
			if (!_context.Employees.Any(j => j.EmployeeID == job.EmployeeId))
			{
				errorList.Add(new Exception("Employee ID not found in database"));
			}

			Job employeeId = _context.Jobs
								.Where(j => j.EmployeeID == job.EmployeeId)
								.Select(j => j).First();

			if (job.ShopRate == 0)
			{
				errorList.Add(new Exception("No Shop Rate identified"));
			}

			if (string.IsNullOrWhiteSpace(job.VehicleIdentificationNumber))
			{
				errorList.Add(new Exception("VIN not provided"));
			}

			if (!_context.Jobs.Any(j => j.VehicleIdentification == job.VehicleIdentificationNumber))
			{
				errorList.Add(new Exception("VIN not found in database"));
			}

			Job vin = _context.Jobs
						.Where(j => j.VehicleIdentification == job.VehicleIdentificationNumber)
						.Select(j => j).First();

			if (errorList.Count == 0)
			{
				newJobRequest.ShopRate = 65.50m;
				newJobRequest.EmployeeID = employeeId.EmployeeID;
				newJobRequest.VehicleIdentification = vin.VehicleIdentification;
				newJobRequest.JobDateIn = DateTime.Now;
			}

			#endregion

			#region Job Detail Validation
			if (jobDetails.Count < 0)
			{
				throw new ArgumentNullException("No job details provided");
			}

			foreach (var jobDetail in jobDetails)
			{

				if (string.IsNullOrWhiteSpace(jobDetail.Description))
				{
					errorList.Add(new Exception($"Service description is needed"));
				}

				JobDetail description = _context.JobDetails
											.Where(jb => jb.Description == jobDetail.Description)
											.Select(jb => jb).First();

				if (jobDetail.JobHour <= 0)
				{
					errorList.Add(new Exception($"Hours for job detail {jobDetail.Description} must be a positive value"));
				}
				JobDetail jobHours = _context.JobDetails
										.Where(jb => jb.JobHours == jobDetail.JobHour)
										.Select(jb => jb).First();

				
				Coupon coupon = _context.Coupons
									.Where(c => c.CouponID == jobDetail.CouponId)
									.Select(x => x).First();

				if (coupon == null)
				{
					coupon = new Coupon()
					{
						CouponID = jobDetail.CouponId
					};
					_context.Coupons.Add(coupon);
				}
				/*if (!_context.Coupons.Any(c => c.CouponID == jobDetail.CouponId))
				{
					errorList.Add(new Exception("Coupon ID not found"));

				}*/

				if (errorList.Count == 0)
				{


					newJobDetails.Description = description.Description;
					newJobDetails.JobHours = jobHours.JobHours;
					newJobDetails.Comments = jobDetail.Comment;
					newJobDetails.CouponID = coupon.CouponID;
					newJobDetails.Coupon.CouponDiscount = jobDetail.DiscountPercent;

					newJobRequest.JobDetails.Add(newJobDetails);

				}
			}
			#endregion

			#region Total Amount Update

			//? Total is estimated time and labour cost

			decimal SubTotal = newJobRequest.ShopRate * newJobDetails.JobHours;
			decimal TaxAmount = job.SubTotal * (5 / 100);
			decimal Total = SubTotal + TaxAmount;

			_context.Jobs.Add(newJobRequest);
			#endregion

			if (errorList.Count() > 0)
			{
				throw new AggregateException("Unable to register a new job", errorList);
			}
			else
			{
				_context.SaveChanges();
			}
		}
	}

}
