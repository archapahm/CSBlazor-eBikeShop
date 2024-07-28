using Microsoft.AspNetCore.Components;
using SalesSystem.BLL;
using SalesSystem.ViewModels;
using ServicingSystem.Entities;

namespace eBike.Pages.SalesPages
{
    public partial class Sales
    {
        [Inject]
        protected SalesServices? SalesServices { get; set; }

		#region fields
		private List<CategoryView> Categories { get; set; }
        private List<PartView> Parts { get; set; }
        private List<SaleDetailView> Cart { get; set; }
        private CouponView Coupon { get; set; }
        private int PartSelection { get; set; }
        private int? StockQuantity { get; set; }
        private int ChosenQuantity { get; set; }
        private int TableItemStock { get; set; }
        private int Discount { get; set; }
        private bool showPopup { get; set; }
        private string Reason { get; set; }
        private bool isDisabled { get; set; }
        private string PopupMessage { get; set; }
        private string PaymentType { get; set; }
        private string CouponCode { get; set; }
        private string CategorySelection { get; set; }
		#endregion

		protected override async Task OnInitializedAsync()
        {
            CategorySelection = "0";

			PartSelection = 0;
            Discount = 0;
            isDisabled = false;
            Categories = await SalesServices.Sale_GetCategories();
            await InvokeAsync(StateHasChanged);
        }
		private void ShowPopupClear()
		{
            PopupMessage = "Are you sure you want to clear your cart?";
			Reason = "clear";
			showPopup = true;
		}
        private void ShowPopupError(string errMsg)
        {
            PopupMessage = errMsg;
            Reason = "error";
            showPopup = true;
        }
        private void ShowPopupSuccess(int saleId)
        {
			PopupMessage = $"Success! Your transaction ID is {saleId}";
			Reason = "success";
			isDisabled = true;
			showPopup = true;
		}
        private void ShowPopupStock(int partID)
        {
			PopupMessage = $"Sorry, {Parts.Where(x=>x.PartID == partID).Select(x=>x.Description).First()} is out of stock";
			Reason = "stock";
			showPopup = true;
		}
		private void ClosePopupStock()
		{
			StockQuantity = null;
			showPopup = false;
		}
		private void ClosePopup()
		{
			showPopup = false;
		}
        private void VerifyCoupon()
        {
            bool exists = SalesServices.VerifyCoupon(CouponCode);
            if (!exists)
            {
                ShowPopupError("Coupon not found");
				return;
            }
            else
            {
                AddCoupon();
            }
		}   
        private void AddCoupon()
        {
		    Coupon = SalesServices.GetCoupon(CouponCode);
            if(Coupon.CouponType != 1)
            {
                ShowPopupError($"{CouponCode} is not applicable to sales");
				return;
            }
            if(Coupon.Start > DateTime.Now)
            {
				ShowPopupError($"{CouponCode} is not yet valid");
                return;
            }
            if (Coupon.End < DateTime.Now)
            {
                ShowPopupError($"{CouponCode} has expired");
                return;
            }
            Discount = Coupon.CouponDiscount;

		}
		private async void ClearCart()
		{
			isDisabled = false;
			Cart = null;
            PartSelection = 0;
            Parts = null;
            PaymentType = null;
            StockQuantity = null;
            ChosenQuantity = 0;
			showPopup = false;
            CategorySelection = "0";
			await InvokeAsync(StateHasChanged);
		}
        private void ChangePaymentType(ChangeEventArgs e)
        {
			PaymentType = e.Value.ToString();
		}
		private async Task PopulateParts(ChangeEventArgs e)
        {
            PartSelection = 0;
            StockQuantity = null;
			int catID = Convert.ToInt32(e.Value.ToString());
            Parts = await SalesServices.Sale_GetParts(catID);
            await InvokeAsync(StateHasChanged);
        }
        private async Task<int> GetStockByInt(int partID)
        {
			return SalesServices.Sale_GetStockInt(partID);
		}
        private async Task GetStock(ChangeEventArgs e)
        {
            ChosenQuantity = 1;
			int partID = Convert.ToInt32(e.Value.ToString());
			StockQuantity = await SalesServices.Sale_GetStock(partID);
			await InvokeAsync(StateHasChanged);
        }
        private async void UpdateCartQuantity(int qty, int partID)
        {
            if(qty > SalesServices.Sale_GetStockInt(partID))
            {
				ShowPopupError("Quantity selected exceeds available stock");
				return;
			}
            SaleDetailView item = Cart.Where(p => p.PartID == partID).FirstOrDefault();
            item.Quantity = qty;
			item.Total = item.Quantity * item.SellingPrice;
			await InvokeAsync(StateHasChanged);
		}
		private void RemoveFromCart(int partID)
        {
            SaleDetailView itemToRemove = Cart.Where(p => p.PartID == partID).FirstOrDefault();
			Cart.Remove(itemToRemove);
		}
        private void GetTableMaxStock(int partID)
        {
            TableItemStock = SalesServices.Sale_GetStockInt(partID);
        }
        private void AddToCart()
        {
            if(CategorySelection == "0")
            {
                ShowPopupError("Please select a category");
				return;
            }
            if(PartSelection == 0)
            {
				ShowPopupError("Please select a part");
                return;
            }
			if (ChosenQuantity > SalesServices.Sale_GetStockInt(PartSelection))
			{
				ShowPopupError("Quantity selected exceeds available stock");
				return;
			}
			if (ChosenQuantity > 0)
            {
                if(Cart == null)
                {
					Cart = new List<SaleDetailView>();
				}
                else
                {
                    if(Cart.Exists(p => p.PartID == PartSelection))
                    {
                        ShowPopupError("This part is already in the cart");
                        return;
					}
                }
                PartView part = Parts.Find(p => p.PartID == PartSelection);
				SaleDetailView newItem = new SaleDetailView
                {
					PartID = part.PartID,
					Description = part.Description,
					Quantity = ChosenQuantity,
					SellingPrice = part.SellingPrice,
					Total = ChosenQuantity * part.SellingPrice
				};
				Cart.Add(newItem);
                return;
            }
			Reason = "error";
			ShowPopupError("Invalid quantity");
			return;
        }
        private void Checkout()
        {
			if(Cart == null)
            {
                ShowPopupError("Cart is empty");
				return;
            }
            else if(PaymentType == null || PaymentType == "x")
            {
				ShowPopupError("Please select a payment type");
                return;
            }
            else
            {
				SaleView newSale = new SaleView
				{
					PaymentType = PaymentType,
					SubTotal = Cart.Sum(p => p.Total),
					DiscountPercent = Discount,
					CouponId = Coupon != null? Coupon.CouponId : null,
					EmployeeId = 1,
					TaxAmount = Cart.Sum(p => p.Total) * (decimal)0.05,
				};
                try
                {
					int newId = SalesServices.Checkout(newSale, Cart);
                    ShowPopupSuccess(newId);
				}
                catch (Exception e)
                {
                    ShowPopupError(e.ToString());
                }
				
			}
        }
    }
}
