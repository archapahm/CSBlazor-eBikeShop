#nullable enable
using Microsoft.AspNetCore.Components;
using PurchasingSystem.BLL;
using PurchasingSystem.ViewModels;

namespace eBike.Pages.PurchasingPages
{
    public partial class PurchaseOrder
    {
        [Inject]
        protected PurchasingService? PurchasingService { get; set; }
        [Inject]
        protected NavigationManager? NavigationManager { get; set; }

        private VendorView? vendor { get; set; } = new();
        private List<VendorView> vendorList { get; set; } = new();
        private PurchaseOrderView purchaseOrder { get; set; } = new();
        private List<VendorInventoryView> vendorInventoryList { get; set; } = new();
        private bool HasActiveOrder { get; set; }

        #region error fields
        // placeholder for feedback messages
        private string feedBackMessage { get; set; }
        // placeholder for error messages
        private string errorMessage { get; set; }
        //a get property that returns the result of the lamda action
        private bool hasError => !string.IsNullOrWhiteSpace(errorMessage);
        private bool hasFeedBack => !string.IsNullOrWhiteSpace(feedBackMessage);
        //used to display any collection of errors on web page
        //whether the errors are generated locally OR come form the class library
        //      service methods
        private List<string> errorDetails { get; set; } = new();
        #endregion error fields

        protected override async Task OnInitializedAsync()
        {
            vendorList = PurchasingService.GetVendors();
        }

        private async Task Save()
        {
            try
            {
                ClearErrorMessage();

                // hard code the employee id
                if (purchaseOrder.EmployeeID == 0) 
                {
                    purchaseOrder.EmployeeID = 7;
                }

                // call our service methods from the BLL
                PurchasingService.SavePurchaseOrder(purchaseOrder);

                // update the feedback with a successful message
                feedBackMessage = $"Purchase Order was successfully saved.";
            }
            catch (ArgumentNullException ex)
            {
                errorMessage = GetInnerException(ex).Message;
            }
            catch (ArgumentException ex)
            {

                errorMessage = GetInnerException(ex).Message;
            }
            catch (AggregateException ex)
            {
                // having collected a number of errors
                // each error should be places into a separate line
                errorMessage = "Unable to add or update the purchase order.";
                foreach (var error in ex.InnerExceptions)
                {
                    errorDetails.Add(error.Message);
                }
            }
            catch (Exception ex)
            {
                errorMessage = GetInnerException(ex).Message;
            }
        }

        private async Task PlaceOrder()
        {
            try
            {
                ClearErrorMessage();

                // hard code the employee id
                if (purchaseOrder.EmployeeID == 0)
                {
                    purchaseOrder.EmployeeID = 7;
                }

                purchaseOrder.OrderDate = DateTime.Now;

                // call our service methods from the BLL
                PurchasingService.SavePurchaseOrder(purchaseOrder);

                // update the feedback with a successful message
                feedBackMessage = $"Purchase Order was successfully placed.";
            }
            catch (ArgumentNullException ex)
            {
                errorMessage = GetInnerException(ex).Message;
            }
            catch (ArgumentException ex)
            {

                errorMessage = GetInnerException(ex).Message;
            }
            catch (AggregateException ex)
            {
                // having collected a number of errors
                // each error should be places into a separate line
                errorMessage = "Unable to add or update the purchase order.";
                foreach (var error in ex.InnerExceptions)
                {
                    errorDetails.Add(error.Message);
                }
            }
            catch (Exception ex)
            {
                errorMessage = GetInnerException(ex).Message;
            }
        }

        private async Task Delete()
        {
            try
            {
                ClearErrorMessage() ;

                //  call our service methods from the BLL
                PurchasingService.DeletePurchaseOrder(purchaseOrder.PurchaseOrderID);

                //  update the feedback with a successful message
                feedBackMessage = $"Purchase Order Number ${purchaseOrder.PurchaseOrderNumber} was successfully deleted.";

                await InvokeAsync(StateHasChanged);
            }
            catch (ArgumentNullException ex)
            {
                errorMessage = GetInnerException(ex).Message;
            }
            catch (ArgumentException ex)
            {

                errorMessage = GetInnerException(ex).Message;
            }
            catch (AggregateException ex)
            {
                //  having collected a number of errors
                //  each error should be places into a separate line
                errorMessage = "Unable to delete purchase order";
                foreach (var error in ex.InnerExceptions)
                {
                    errorDetails.Add(error.Message);
                }
            }
            catch (Exception ex)
            {
                errorMessage = GetInnerException(ex).Message;
            }
        }

        private void ClearPurchaseOrder()
        {
            NavigationManager.NavigateTo(NavigationManager.Uri, forceLoad: true);
        }

        private void DisplayVendorInfo(ChangeEventArgs e)
        {
            ClearErrorMessage();
            int vendorId = Convert.ToInt32(e.Value);
            if (vendorId != 0)
            {
                vendor = vendorList.Where(v =>  v.VendorID == vendorId).FirstOrDefault();
                purchaseOrder = PurchasingService.GetPurchaseOrder(vendorId);
                vendorInventoryList = PurchasingService.GetVendorInventory(vendorId);
            }
            CalculatePurchaseOrder();

            HasActiveOrder = purchaseOrder.PurchaseOrderNumber != 0 ? true : false;
        }

        private void RemovePurchaseOrderDetail(PurchaseOrderDetailView purchaseOrderDetailView)
        {
            vendorInventoryList.Add(new VendorInventoryView
            {
                PartID = purchaseOrderDetailView.PartID,
                Description = purchaseOrderDetailView.Description,
                QuantityOnHand = purchaseOrderDetailView.QuantityOnHand,
                ReorderLevel = purchaseOrderDetailView.ReorderLevel,
                QuantityOnOrder = purchaseOrderDetailView.QuantityOnOrder,
                Buffer = ((purchaseOrderDetailView.QuantityOnHand + purchaseOrderDetailView.QuantityOnOrder) - purchaseOrderDetailView.ReorderLevel) > 0 ? ((purchaseOrderDetailView.QuantityOnHand + purchaseOrderDetailView.QuantityOnOrder) - purchaseOrderDetailView.ReorderLevel) : 0,
                PurchasePrice = purchaseOrderDetailView.PurchasePrice
            });
            purchaseOrder.PurchaseOrderDetails.Remove(purchaseOrderDetailView);
            CalculatePurchaseOrder();
        }

        private void AddPurchaseOrderDetail(VendorInventoryView vendorInventoryView)
        {
            purchaseOrder.PurchaseOrderDetails.Add(new PurchaseOrderDetailView
            {
                PartID = vendorInventoryView.PartID,
                Description = vendorInventoryView.Description,
                QuantityOnHand = vendorInventoryView.QuantityOnHand,
                ReorderLevel = vendorInventoryView.ReorderLevel,
                QuantityOnOrder = vendorInventoryView.QuantityOnOrder,
                QuantityToOrder = (vendorInventoryView.ReorderLevel - (vendorInventoryView.QuantityOnHand + vendorInventoryView.QuantityOnOrder)) > 0 ? (vendorInventoryView.ReorderLevel - (vendorInventoryView.QuantityOnHand + vendorInventoryView.QuantityOnOrder)) : 0,
                PurchasePrice = vendorInventoryView.PurchasePrice
            });
            vendorInventoryList.Remove(vendorInventoryView);
            CalculatePurchaseOrder();
        }

        private void RefreshPurchaseOrderDetailInput(PurchaseOrderDetailView purchaseOrderDetailView)
        {
            purchaseOrderDetailView.QuantityToOrder = 0;
            purchaseOrderDetailView.PurchasePrice = 0;
            CalculatePurchaseOrder();
        }

        private void CalculateOnChangeQuantity(int partId, object? value)
        {
            if (value != null)
            {
                var quantity = Convert.ToInt32(value);
                var item = purchaseOrder.PurchaseOrderDetails.FirstOrDefault(x => x.PartID == partId);
                if (item != null)
                {
                    item.QuantityToOrder = quantity;
                    CalculatePurchaseOrder();
                }
            }
        }

        private void CalculateOnChangePrice(int partId, object? value)
        {
            if (value != null)
            {
                var price = Convert.ToDecimal(value);
                var item = purchaseOrder.PurchaseOrderDetails.FirstOrDefault(x => x.PartID == partId);
                if (item != null)
                {
                    item.PurchasePrice = price;
                    CalculatePurchaseOrder();
                }
            }
        }

        private void CalculatePurchaseOrder()
        {
            purchaseOrder.SubTotal = purchaseOrder.PurchaseOrderDetails.Sum(pod => pod.QuantityToOrder * pod.PurchasePrice);
            purchaseOrder.TaxAmount = purchaseOrder.SubTotal * 0.05m;
        }

        private void ClearErrorMessage()
        {
            // reset the error detail list
            errorDetails.Clear();

            // reset error message to an empty string
            errorMessage = string.Empty;

            // reset feedback message to an empty string
            feedBackMessage = string.Empty;
        }

        private Exception GetInnerException(Exception ex)
        {
            while (ex.InnerException != null)
                ex = ex.InnerException;
            return ex;
        }
    }
}
