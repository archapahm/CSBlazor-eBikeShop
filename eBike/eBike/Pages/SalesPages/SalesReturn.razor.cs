using Microsoft.AspNetCore.Components;
using SalesSystem.BLL;
using SalesSystem.ViewModels;
using System.Diagnostics.Eventing.Reader;

namespace eBike.Pages.SalesPages
{
    public partial class SalesReturn
    {
        [Inject]
        protected SalesServices? SalesServices { get; set; }

		#region fields
        private int SaleID { get; set; }
        private bool isDisabled { get; set; }
        private bool isClearDisabled { get; set; }
        private SaleRefundView Sale { get; set; }
        private List<SaleRefundDetailView> Details { get; set; }
		private string PopupMessage { get; set; }
		private string Reason { get; set; }
		private bool showPopup { get; set; }
		private int? RefundID { get; set; }
		#endregion 

        protected override async Task OnInitializedAsync()
        {
			SaleID = 0;
			isDisabled = false;
			isClearDisabled = true;
		}

        private async Task SearchSale()
        {
            try
            {
				Sale =  SalesServices.SaleRefund_GetSaleRefund(SaleID).Result;
				Details = SalesServices.SaleRefund_GetSaleDetailsRefund(SaleID);
				isClearDisabled = false;
				await InvokeAsync(StateHasChanged);
			}
            catch (Exception ex)
            {
				ShowPopupError(ex.InnerException.Message);
			}
		}
		private async Task ClearSale()
		{
			try
			{
				RefundID = null;
				isDisabled = false;
				isClearDisabled = true;
				Sale = null;
				Details = null;
				SaleID = 0;
				ClosePopup();
				await InvokeAsync(StateHasChanged);

			}
			catch (Exception ex)
			{
				ShowPopupError(ex.Message);
			}
		}
		private void ProcessRefund()
		{
			try
			{
				if(Details.Sum(x=>x.Quantity) == 0)
				{
					ShowPopupError("No items selected for refund");
					return;
				}
				int refundId = SalesServices.SaleRefund_Refund(Sale, Details);
				isDisabled = true;
				RefundID = refundId;
				ShowPopupSuccess(refundId);
			}
			catch (AggregateException ex)
			{
				string err = "";
				int count = 0;
				foreach(var e in ex.InnerExceptions)
				{
					if (count == 0)
					{
						err += e.Message;
					}
					else
					{
						err += $", {e.Message}";
					}
					count++;
				}
				ShowPopupError(err);
			}
			catch (Exception ex)
			{
				ShowPopupError(ex.InnerException.Message);
			}			
		}
		private void ShowPopupClear()
		{
			PopupMessage = "Are you sure you want to clear?";
			Reason = "clear";
			showPopup = true;
		}
		private void ShowPopupError(string errMsg)
		{
			PopupMessage = errMsg;
			Reason = "error";
			showPopup = true;
		}
		private void ShowPopupSuccess(int refundId)
		{
			PopupMessage = $"Success! Your Refund ID is {refundId}";
			Reason = "success";
			isDisabled = true;
			isClearDisabled = false;
			showPopup = true;
		}
		private void ClosePopup()
		{
			showPopup = false;
		}
	}
}
