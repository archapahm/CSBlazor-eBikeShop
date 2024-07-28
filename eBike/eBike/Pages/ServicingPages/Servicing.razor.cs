using Microsoft.EntityFrameworkCore;
using ServicingSystem.ViewModels;
using ServicingSystem.BLL;
using Microsoft.AspNetCore.Components;
using PurchasingSystem.BLL;
using PurchasingSystem.ViewModels;
using SalesSystem.BLL;
using ServicingSystem.Entities;

namespace eBike.Pages.ServicingPages
{
	public partial class Servicing
	{
		[Inject] protected ServicingService? ServicingService { get; set; }
		private string customerLastName { get; set; }

		protected List<CustomerView> Customers { get; set; } = new();

        protected List<CustomerVehicleView> Vehicles { get; set; } = new();

		protected CustomerVehicleView? CustomerVehicle { get; set; } 

        private List<string> serviceList { get; set; } = new();

        private string vin { get; set; }

		private string description { get; set; }

		private string couponCode { get; set; }

		private int CouponDiscount { get; set; }

		private decimal jobHours { get; set; }

		private string comment { get; set; }

		private int jobDetailId { get; set; }

        private List<JobDetailView>? JobDetailList { get; set; }

        private decimal totalJobHours { get; set; } 

		private decimal subtotal { get; set; }
		private decimal shopRate { get; set; } = 65.5M;


        private async Task GetCustomer()
		{
			Customers = await ServicingService.GetCustomer(customerLastName);
			await InvokeAsync(StateHasChanged);
		}

		private async Task GetCustomerVehicle(int customerId)
		{
			Vehicles = await ServicingService.GetCustomerVehicle(customerId);
			//await InvokeAsync(StateHasChanged);
		}

		private async Task GetCoupon()
		{
			CouponDiscount = await ServicingService.GetCoupon(couponCode);
		}

		private void AddService()
		{
			if(JobDetailList == null)
			{
				JobDetailList = new List<JobDetailView>();
			}

			JobDetailView newService = new JobDetailView
			{
				JobDetailID = jobDetailId,
				Description = description,
				JobHour = jobHours,
				Comment = comment,
				DiscountPercent = CouponDiscount / 100
			};

			JobDetailList.Add(newService);
			Clear();
			return;
		}

		private void RemoveService(JobDetailView jobDetailView)
		{
			JobDetailList.Remove(jobDetailView);
		}


        private void SetVin(ChangeEventArgs e)
		{
			 vin = e.Value.ToString();
			
		}

		private void SetSubtotal(ChangeEventArgs e)
		{
			subtotal = (decimal)e.Value;
		}

		private void SetServiceDescription(ChangeEventArgs e)
		{
			description = e.Value.ToString();
		}

		private void SetTotalHours(ChangeEventArgs e)
		{
			totalJobHours = (decimal)e.Value;
		}

        protected override async Task OnInitializedAsync()
        {
            serviceList = ServicingService.GetStandardService();
        }

        private Exception GetInnerException(Exception ex)
        {
            while (ex.InnerException != null)
                ex = ex.InnerException;
            return ex;
        }

		private async void Clear()
		{
			customerLastName = null;
			vin = null;
			description = null;
			couponCode = null;
			comment = null;
			jobHours = 0;
			Customers.Clear();
			Vehicles.Clear();

			
		}

		private async void ClearAll()
		{
            customerLastName = null;
            vin = null;
            description = null;
            couponCode = null;
            comment = null;
            Customers.Clear();
            Vehicles.Clear();
			JobDetailList = null;
			totalJobHours = 0;
			subtotal = 0;
        }
		private void RegisterJob()
		{
			if(JobDetailList == null)
			{
				throw new ArgumentNullException("No Job Detail Provided");
			}
			else
			{
                JobView newjob = new JobView
                {
                    EmployeeId = 1,
                    ShopRate = 65.5M,
                    VehicleIdentificationNumber = vin,
                    TaxAmount = (decimal)(subtotal / 0.5M),
                    SubTotal = (decimal)subtotal - CouponDiscount,
                    JobDateIn = DateTime.Now
                };

                ServicingService.RegisterJob(newjob, JobDetailList);
            }
			

		}

    }
}
