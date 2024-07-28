#nullable enable
using Microsoft.AspNetCore.Components;
using ReceivingSystem.BLL;
using ReceivingSystem.ViewModels;

namespace eBike.Pages.ReceivingPages
{
    public partial class Receiving
    {
        [Inject]
        protected ReceivingService? ReceivingService { get; set; }
        [Inject]
        protected NavigationManager? NavigationManager { get; set; }


        private List<OutstandingOrdersItemView>? OutstandingOrdersModel { get; set; } = new List<OutstandingOrdersItemView>();
    
        private int UOQuantity { get; set; }
        private string UOVendorPartNumber { get; set; }
        private string UODescription { get; set; } 
        private List<UnOrderedItemView>? UnOrderedItemsModel { get; set; } = new List<UnOrderedItemView>();
        private OrderDetailsView? ReceiveOrderModel { get; set; } = null;
        private List<Exception> ErrorList { get; set; } = new List<Exception>();
        private bool? SuccessModel { get; set; }
        private int? SucessOrderID { get; set; }
        protected override async Task OnInitializedAsync()
        {
            OutstandingOrdersModel = ReceivingService.GetOutstandingOrders();
        }

        private void ViewOrder(int orderID)
        {
            if (orderID <= 0)
            {
                ErrorList.Add(new Exception($"Invalid Order ID (OrderID cannot be equal to {orderID})"));
                SuccessModel = false;
            }
            else
            {
                OutstandingOrdersModel = null;
                var temp = ReceivingService.GetOrderDetails(orderID);
                temp.ItemDetails
                    .ForEach(x => {
                        int oq = (x.Quantity - (x.ReceivedQuantity ?? 0)) + (x.ReturnQuantity ?? 0);
                        if (oq < 0)
                        {
                            oq = 0;
                        }
                        x.OutstandingQuantity = oq;
                    });
                ReceiveOrderModel = temp;
                UnOrderedItemsModel = new List<UnOrderedItemView>();
            }
        }
        private void DeleteUnOrderedItem(UnOrderedItemView item)
        {
            UnOrderedItemsModel.Remove(item);
        }
        private void AddUnOrderedItem()
        {
            if (UOQuantity <= 0)
            {
                ErrorList.Add(new Exception($"Invalid Quantity (Quantity cannot be equal to {UOQuantity})"));
                SuccessModel = false;
            }
            if (string.IsNullOrWhiteSpace(UOVendorPartNumber))
            {
                ErrorList.Add(new Exception($"Invalid Vendor Part Number (Vendor Part Number cannot be empty)"));
                SuccessModel = false;
            }
            if (string.IsNullOrWhiteSpace(UODescription))
            {
                ErrorList.Add(new Exception($"Invalid Description (Description cannot be empty)"));
                SuccessModel = false;
            }
            
            if (ErrorList.Count > 0)
            {
                SuccessModel = false;
            } else {
                UnOrderedItemsModel.Add(new UnOrderedItemView
                {
                    Description = UODescription,
                    Quantity = UOQuantity,
                    VendorPartNumber = UOVendorPartNumber
                });
                UOQuantity = 0;
                UODescription = "";
                UOVendorPartNumber = "";
                SuccessModel = true;

            }
            
            
        }
        private string? ForceCloseReason { get; set; }
        private void ForceClose()
        {
            if (string.IsNullOrWhiteSpace(ForceCloseReason))
            {
                ErrorList.Add(new Exception($"Invalid Reason (Reason cannot be empty)"));
                SuccessModel = false;
            }
            else
            {
                try
                {
                    ReceivingService.ForceCloseOrder(ReceiveOrderModel.PurchaseOrderId, ForceCloseReason, ReceiveOrderModel.EmployeeId);
                }
                catch (System.AggregateException ex)
                {
                    foreach (var item in ex.InnerExceptions)
                    {
                        ErrorList.Add(item);
                    }
                }
            }
            if (ErrorList.Count > 0)
            {
                SuccessModel = false;
            }
            else
            {
                SuccessModel = true;
                OutstandingOrdersModel = ReceivingService.GetOutstandingOrders();
                ReceiveOrderModel = null;
                UnOrderedItemsModel = null;
                SucessOrderID = null;
            }
        }
        private void ClearInputs()
        {
            ReceiveOrderModel = ReceivingService.GetOrderDetails(ReceiveOrderModel.PurchaseOrderId);
            UnOrderedItemsModel = null;
            ErrorList = new List<Exception>();
            SucessOrderID = null;
            UnOrderedItemsModel = null;
            SuccessModel = true;
        }
        
        private void ReceiveOrder()
        {
            if (ReceiveOrderModel == null)
            {
                ErrorList.Add(new Exception("No Order to receive"));
            }
            else
            {
                try
                {
                    if (UnOrderedItemsModel != null)
                    {
                        ReceivingService.ReceiveOrder(ReceiveOrderModel, UnOrderedItemsModel);
                    }
                    else
                    {
                        ReceivingService.ReceiveOrder(ReceiveOrderModel);
                    }
                }
                catch (System.AggregateException ex)
                {
                    foreach (var item in ex.InnerExceptions)
                    {
                        ErrorList.Add(item);
                    }
                }
                // catch (Exception ex)
                // {
                //     ErrorList.Add(ex);
                // }

            }

            if (ErrorList.Count > 0)
            {
                SuccessModel = false;
            } 
            else
            {
                var temp = ReceivingService.GetOutstandingOrders();
                SucessOrderID = ReceiveOrderModel.PurchaseOrderId;
                ReceiveOrderModel = null;
                OutstandingOrdersModel = temp;
                SuccessModel = true;
            }
        }
    
    }
}