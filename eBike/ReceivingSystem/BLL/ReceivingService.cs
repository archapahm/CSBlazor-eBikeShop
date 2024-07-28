#nullable enable
using Microsoft.EntityFrameworkCore;
using ReceivingSystem.DAL;
using ReceivingSystem.Entities;
using ReceivingSystem.ViewModels;
// using System;
// using System.Collections.Generic;
// using System.Linq;
// using System.Text;
// using System.Threading.Tasks;

namespace ReceivingSystem.BLL
{
    public class ReceivingService
    {
        private readonly ReceivingContext _context;

        internal ReceivingService(ReceivingContext context)
        {
            _context = context;
        }

        public void ForceCloseOrder(int PurchaseOrderID, string ForceCloseReason, int EmployeeID)
        {
            List<Exception> ErrorList = new List<Exception>();
            if (PurchaseOrderID <= 0)
            {
                ErrorList.Add(new ArgumentException("PurchaseOrderID cannot be equal to or less than 0"));
            }
            if (string.IsNullOrWhiteSpace(ForceCloseReason))
            {
                ErrorList.Add(new ArgumentNullException("A reason must be given to force close a purchase order."));
            }
            if (!_context.PurchaseOrders.Any(po => po.PurchaseOrderID == PurchaseOrderID))
            {
                ErrorList.Add(new ArgumentException($"An Order with the ID:{PurchaseOrderID} does not exist"));
            }


            if (!_context.Employees.Any(e => e.EmployeeID == EmployeeID))
            {
                ErrorList.Add(new ArgumentException($"An Employee with the ID:{EmployeeID} does not exist"));
            }

            if (ErrorList.Count() > 0)
            {
                throw new AggregateException(ErrorList);
            }

            PurchaseOrder matchingOrder = _context.PurchaseOrders
                .Where(ro => ro.PurchaseOrderID == PurchaseOrderID && ro.EmployeeID == EmployeeID)
                .Single();
            matchingOrder.Closed = true;
            matchingOrder.Notes = ForceCloseReason;

            _context.SaveChanges();
        }

        public OrderDetailsView GetOrderDetails(int OrderId)
        {
            return _context.PurchaseOrders
                .Where(po => po.PurchaseOrderID == OrderId)
                .Select(po => new OrderDetailsView
                {
                    PurchaseOrderId = po.PurchaseOrderID,
                    VendorName = po.Vendor.VendorName,
                    VendorPhone = po.Vendor.Phone,
                    EmployeeId = po.EmployeeID,
                    ItemDetails = _context.PurchaseOrderDetails
                        .Where(pod => pod.PurchaseOrderID == OrderId)
                        .Select(pod => new OrderItemDetailsView
                        {
                            PurchaseOrderDetailsID = pod.PurchaseOrderDetailID,
                            PartID = pod.PartID,
                            Description = pod.Part.Description,
                            Quantity = pod.Quantity,
                            ReceivedQuantity = pod
                                .ReceiveOrderDetails
                                .Single(rod => rod.PurchaseOrderDetailID == pod.PurchaseOrderDetailID)
                                .QuantityReceived,
                            ReturnQuantity = pod
                                .ReturnedOrderDetails
                                .Single(rod => rod.PurchaseOrderDetailID == pod.PurchaseOrderDetailID)
                                .Quantity,
                            ReturnReason = pod
                                .ReturnedOrderDetails
                                .Single(rod => rod.PurchaseOrderDetailID == pod.PurchaseOrderDetailID)
                                .Reason
                        })
                        .ToList()
                })
                .Single();
        }
        public List<OutstandingOrdersItemView> GetOutstandingOrders()
        {
            return _context.PurchaseOrders
                .Where(po => po.Closed == false)
                .Select(po => new OutstandingOrdersItemView
                {
                    PurchaseOrderId = po.PurchaseOrderID,
                    OrderDate = po.OrderDate,
                    VendorName = po.Vendor.VendorName,
                    Phone = po.Vendor.Phone
                })
                .ToList();
        }
        public List<UnOrderedItemView> GetUnOrderedItems(int cartId)
        {
            return _context.UnorderedPurchaseItemCarts
                .Where(upic => upic.CartID == cartId)
                .Select(upic => new UnOrderedItemView
                {
                    VendorPartNumber = upic.VendorPartNumber,
                    Description = upic.Description,
                    Quantity = upic.Quantity
                })
                .ToList();
        }

        public void InsertUOItem(UnOrderedItemView unorderedItem)
        {
            List<Exception> ExceptionList = new List<Exception>();

            if (string.IsNullOrWhiteSpace(unorderedItem.VendorPartNumber))
            {
                ExceptionList.Add(new ArgumentNullException("The given VendorPartNumber cannot be a null, whitespace, or empty value"));
            }
            if (string.IsNullOrWhiteSpace(unorderedItem.Description))
            {
                ExceptionList.Add(new ArgumentNullException("The given Description cannot be a null, whitespace, or empty value"));
            }
            if (unorderedItem.Quantity <= 0)
            {
                ExceptionList.Add(new ArgumentOutOfRangeException("The given Quantity cannot be equal to or less than 0"));
            }

            if (ExceptionList.Count() > 0)
            {
                throw new AggregateException("Unable to insert item, there were errors during processing.", ExceptionList);
            }
            //no errors past this point

            _context.UnorderedPurchaseItemCarts.Add(new UnorderedPurchaseItemCart
            {
                VendorPartNumber = unorderedItem.VendorPartNumber,
                Description = unorderedItem.Description,
                Quantity = unorderedItem.Quantity
            });

            _context.SaveChanges();
        }

        public void ReceiveOrder(OrderDetailsView orderDetails, List<UnOrderedItemView> unOrderedItems)
        {
            foreach (UnOrderedItemView item in unOrderedItems)
            {
                InsertUOItem(item);
            }
            ReceiveOrder(orderDetails);
        }
        public void ReceiveOrder(OrderDetailsView orderDetails)
        {
            List<Exception> Errorlist = new List<Exception>();

            PurchaseOrder? matchingOrder = _context.PurchaseOrders
                .Where(po => po.PurchaseOrderID == orderDetails.PurchaseOrderId)
                .SingleOrDefault();

            if (matchingOrder == null)
            {
                Errorlist.Add(new ArgumentException("PurchaseOrderID must already exist"));
            }


            Employee? matchingEmployee = _context.Employees
                .Where(e => e.EmployeeID == orderDetails.EmployeeId)
                .SingleOrDefault();

            if (matchingEmployee == null)
            {
                Errorlist.Add(new ArgumentException("An employee with the given ID must does not exist"));
            }


            ReceiveOrder newOrder = new ReceivingSystem.Entities.ReceiveOrder
            {
                EmployeeID = orderDetails.EmployeeId,
                PurchaseOrder = matchingOrder,
                PurchaseOrderID = matchingOrder.PurchaseOrderID,
                ReceiveDate = DateTime.Now
            };


            List<ReceiveOrderDetail> receiveDetailsToSave = new List<ReceiveOrderDetail>();
            List<ReturnedOrderDetail> returnDetailsToSave = new List<ReturnedOrderDetail>();
            orderDetails.ItemDetails.ForEach(id =>
            {
                if (id.ReceivedQuantity == null) { id.ReceivedQuantity = 0; }
                if (id.ReturnQuantity == null) { id.ReturnQuantity = 0; }

                if (id.ReceivedQuantity > id.Quantity)
                {
                    Errorlist.Add(new ArgumentException($"You cannot receive more items than you ordered (Received {id.ReceivedQuantity} of {id.Quantity} for \"{id.Description}\")"));
                }
                if (id.ReturnQuantity > id.Quantity)
                {
                    Errorlist.Add(new ArgumentException($"You cannot return more items than you currently have (Returned {id.ReturnQuantity} of {id.Quantity} for \"{id.Description}\")"));
                }
                if (id.ReturnQuantity > id.ReceivedQuantity)
                {
                    Errorlist.Add(new ArgumentException($"You cannot return more items than you received (Returned {id.ReturnQuantity} of {id.ReceivedQuantity} for \"{id.Description}\")"));
                }
                if (id.ReceivedQuantity <= 0)
                {
                    Errorlist.Add(new ArgumentException($"You cannot receive an amount of items less than or equal to 0 (Received {id.ReceivedQuantity} of {id.Quantity} for \"{id.Description}\")"));
                }
                if (id.ReceivedQuantity < id.Quantity && string.IsNullOrWhiteSpace(id.ReturnReason))
                {
                    Errorlist.Add(new ArgumentException($"You must provide a reason for returning items (Received {id.ReceivedQuantity} of {id.Quantity} for \"{id.Description}\")"));
                }

                if (Errorlist.Count > 0)
                {
                    throw new AggregateException("Unable to receive order, there were errors during processing.", Errorlist);
                }

                ReceiveOrderDetail newReceiveOrderDetails = new ReceivingSystem.Entities.ReceiveOrderDetail
                {
                    ReceiveOrder = newOrder,
                    PurchaseOrderDetailID = id.PurchaseOrderDetailsID,
                    QuantityReceived = id.ReceivedQuantity.GetValueOrDefault(),

                };
                receiveDetailsToSave.Add(newReceiveOrderDetails);
                if (id.ReturnQuantity > 0)
                {
                    ReturnedOrderDetail newReturnOrderDetails = new ReceivingSystem.Entities.ReturnedOrderDetail
                    {
                        PurchaseOrderDetailID = id.PurchaseOrderDetailsID,
                        ReceiveOrder = newOrder,
                        ItemDescription = id.Description,
                        VendorPartNumber = _context.PurchaseOrderDetails.SingleOrDefault(pod => pod.PurchaseOrderDetailID == id.PurchaseOrderDetailsID).VendorPartNumber ?? "",
                        Quantity = id.ReturnQuantity ?? 0,
                        Reason = id.ReturnReason ?? ""
                    };
                    returnDetailsToSave.Add(newReturnOrderDetails);

                }


            });
            _context.UnorderedPurchaseItemCarts
                .ToList()
                .ForEach(upic => returnDetailsToSave.Add(new ReturnedOrderDetail
                {
                    // PurchaseOrderDetailID = upic
                    ReceiveOrder = newOrder,
                    ItemDescription = upic.Description,
                    VendorPartNumber = upic.VendorPartNumber,
                    Quantity = upic.Quantity,
                    Reason = "Item was not ordered"
                }));

            if (Errorlist.Count() > 0)
            {
                throw new AggregateException(Errorlist);
            }

            _context.ReceiveOrders.Add(newOrder);
            _context.ReceiveOrderDetails.AddRange(receiveDetailsToSave);
            _context.ReturnedOrderDetails.AddRange(returnDetailsToSave);

            _context.SaveChanges();
        }

        public void DeleteUOItem(string PartString)
        {
            List<Exception> ErrorList = new List<Exception>();
            if (string.IsNullOrWhiteSpace(PartString))
            {
                ErrorList.Add(new ArgumentNullException("VendorPartNumber cannot be null/empty/whitespace."));
            }
            UnorderedPurchaseItemCart matchingCart = _context.UnorderedPurchaseItemCarts
                .Where(upic => upic.VendorPartNumber == PartString)
                .SingleOrDefault();
            if (matchingCart == null)
            {
                ErrorList.Add(new ArgumentException($"An Item with the ID:{PartString} does not exist"));
            }
            //after final error possible
            if (ErrorList.Count() > 0)
            {
                throw new AggregateException(ErrorList);
            }
            _context.UnorderedPurchaseItemCarts.Remove(matchingCart);

            _context.SaveChanges();
        }

    }
}
