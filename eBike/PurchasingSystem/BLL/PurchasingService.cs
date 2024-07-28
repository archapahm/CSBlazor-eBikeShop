#nullable enable
using PurchasingSystem.DAL;
using PurchasingSystem.Entities;
using PurchasingSystem.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PurchasingSystem.BLL
{
    public class PurchasingService
    {
        private readonly PurchasingContext _context;

        internal PurchasingService(PurchasingContext context)
        {
            _context=context;
        }

        public List<VendorView> GetVendors()
        {
            return _context.Vendors
                .OrderBy(v => v.VendorName)
                .Select(v => new VendorView
                {
                    VendorID = v.VendorID,
                    VendorName = v.VendorName,
                    Phone = v.Phone,
                    City = v.City
                }).ToList();
        }

        public PurchaseOrderView GetPurchaseOrder(int vendorID)
        {
            if (vendorID == 0)
            {
                throw new ArgumentNullException("Vendor ID was not provided.");
            }

            if (!_context.Vendors.Any(v => v.VendorID == vendorID))
            {
                throw new ArgumentException("Vendor ID does not exist.");
            }

            // Get the current active order for the specified vendor
            PurchaseOrderView purchaseOrder = _context.PurchaseOrders
                .Where(po => po.VendorID == vendorID
                    && po.OrderDate == null)
                .Select(po => new PurchaseOrderView
                {
                    PurchaseOrderID = po.PurchaseOrderID,
                    PurchaseOrderNumber = po.PurchaseOrderNumber,
                    EmployeeID = po.EmployeeID,
                    VendorID = po.VendorID,
                    OrderDate = po.OrderDate.Value,
                    SubTotal = po.SubTotal,
                    TaxAmount = po.TaxAmount,
                    Notes = po.Notes,
                    PurchaseOrderDetails = _context.PurchaseOrderDetails
                        .Where(pod => pod.PurchaseOrderID == po.PurchaseOrderID)
                        .OrderBy(pod => pod.PartID)
                        .Select(pod => new PurchaseOrderDetailView
                        {
                            PurchaseOrderDetailID = pod.PurchaseOrderDetailID,
                            PartID = pod.PartID,
                            Description = pod.Part.Description,
                            QuantityOnHand = pod.Part.QuantityOnHand,
                            ReorderLevel = pod.Part.ReorderLevel,
                            QuantityOnOrder = pod.Part.QuantityOnOrder,
                            QuantityToOrder = pod.Quantity,
                            PurchasePrice = pod.PurchasePrice
                        }).ToList()
                }).FirstOrDefault();

            // If no current active order, get a suggested order
            if (purchaseOrder == null)
            {
                purchaseOrder = new PurchaseOrderView();
                purchaseOrder.VendorID = vendorID;
                purchaseOrder.PurchaseOrderDetails = _context.Parts
                    .Where(p => p.VendorID == vendorID
                        && p.ReorderLevel > (p.QuantityOnHand + p.QuantityOnOrder))
                    .Select(p => new PurchaseOrderDetailView
                    {
                        PartID = p.PartID,
                        Description = p.Description,
                        QuantityOnHand = p.QuantityOnHand,
                        ReorderLevel = p.ReorderLevel,
                        QuantityOnOrder = p.QuantityOnOrder,
                        QuantityToOrder = p.ReorderLevel - (p.QuantityOnHand + p.QuantityOnOrder),
                        PurchasePrice = p.PurchasePrice
                    }).ToList();
            }

            return purchaseOrder;
        }

        public List<VendorInventoryView> GetVendorInventory(int vendorID)
        {
            if (vendorID == 0)
            {
                throw new ArgumentNullException("Vendor ID was not provided.");
            }

            if (!_context.Vendors.Any(v => v.VendorID == vendorID))
            {
                throw new ArgumentException("Vendor ID does not exist.");
            }

            // Get the active order part id(s)
            List<int> purchaseOrderIDs = _context.PurchaseOrderDetails
                .Where(pod => pod.PurchaseOrder.VendorID == vendorID
                    && pod.PurchaseOrder.OrderDate == null)
                .Select(pod => pod.PartID).ToList();

            List<VendorInventoryView> vendorInventoryList;

            // When there is an active order, get only the parts that is not in the purchase order details
            if (purchaseOrderIDs.Count() != 0)
            {
                vendorInventoryList = _context.Parts
                    .Where(p => p.VendorID == vendorID
                        && purchaseOrderIDs.All(poId => poId != p.PartID))
                    .Select(p => new VendorInventoryView
                    {
                        PartID = p.PartID,
                        Description = p.Description,
                        QuantityOnHand = p.QuantityOnHand,
                        ReorderLevel = p.ReorderLevel,
                        QuantityOnOrder = p.QuantityOnOrder,
                        Buffer = ((p.QuantityOnHand + p.QuantityOnOrder) - p.ReorderLevel) <= 0 ? 0 : ((p.QuantityOnHand + p.QuantityOnOrder) - p.ReorderLevel),
                        PurchasePrice = p.PurchasePrice
                    }).ToList();
            }
            // When there is no active order, get only the parts that is not in the suggested order details 
            else
            {
                vendorInventoryList = _context.Parts
                    .Where(p => p.VendorID == vendorID
                        && p.ReorderLevel <= (p.QuantityOnHand + p.QuantityOnOrder))
                    .Select(p => new VendorInventoryView
                    {
                        PartID = p.PartID,
                        Description = p.Description,
                        QuantityOnHand = p.QuantityOnHand,
                        ReorderLevel = p.ReorderLevel,
                        QuantityOnOrder = p.QuantityOnOrder,
                        Buffer = ((p.QuantityOnHand + p.QuantityOnOrder) - p.ReorderLevel) <= 0 ? 0 : ((p.QuantityOnHand + p.QuantityOnOrder) - p.ReorderLevel),
                        PurchasePrice = p.PurchasePrice
                    }).ToList();
            }

            return vendorInventoryList;
        }

        public void SavePurchaseOrder(PurchaseOrderView purchaseOrder)
        {
            List<Exception> errorList = new List<Exception>();
            PurchaseOrder purchaseOrderToSave = new PurchaseOrder();
            
            if (purchaseOrder == null || purchaseOrder.PurchaseOrderDetails.Count() == 0)
            {
                throw new ArgumentNullException("No purchase order or purchase order details submittted.");
            }

            if (purchaseOrder.VendorID == 0)
            {
                errorList.Add(new Exception($"Vendor ID is missing."));
            }

            Vendor vendor = _context.Vendors
                                .Where(v => v.VendorID == purchaseOrder.VendorID)
                                .FirstOrDefault();
            if (vendor == null)
            {
                errorList.Add(new Exception($"Vendor ID {purchaseOrder.VendorID} does not exist in the database."));
            }

            if (purchaseOrder.EmployeeID == 0)
            {
                errorList.Add(new Exception($"Employee ID is missing."));
            }

            Employee employee = _context.Employees
                                    .Where(v => v.EmployeeID == purchaseOrder.EmployeeID)
                                    .FirstOrDefault();
            if (employee == null)
            {
                errorList.Add(new Exception($"Employee ID {purchaseOrder.EmployeeID} does not exist in the database."));
            }

            PurchaseOrder existingPO = new PurchaseOrder();
            List<PurchaseOrderDetail> existingPOD = new List<PurchaseOrderDetail>();

            if (errorList.Count == 0)
            {
                // For active order
                if (purchaseOrder.PurchaseOrderID != 0)
                {
                    
                    // Get existing purchase order details
                    existingPOD = _context.PurchaseOrderDetails
                        .Where(pod => pod.PurchaseOrderID == purchaseOrder.PurchaseOrderID)
                        .Select(pod => pod).ToList();

                    if (existingPOD.Count > 0)
                    {
                        List<int> existingParts = existingPOD.Select(pod => pod.PartID).ToList();
                        List<int> newParts = purchaseOrder.PurchaseOrderDetails.Select(pod => pod.PartID).ToList();
                        List<int> added = newParts.Except(existingParts).ToList();

                        // This is for updating and deleting purchase order detail
                        foreach (PurchaseOrderDetail detail in existingPOD)
                        {
                            PurchaseOrderDetailView podView = purchaseOrder.PurchaseOrderDetails.FirstOrDefault(pod => pod.PartID==detail.PartID);

                            if (podView != null)
                            { 
                                if (podView.QuantityToOrder <= 0)
                                {
                                    errorList.Add(new Exception($"Part {podView.Description} quantity to order must be greater than zero."));
                                }

                                if (podView.PurchasePrice <= 0)
                                {
                                    errorList.Add(new Exception($"Part {podView.Description} price must be greater than zero."));
                                }
                            }

                            if (errorList.Count == 0)
                            {
                                // Delete
                                if (podView == null)
                                {
                                    _context.PurchaseOrderDetails.Remove(detail);
                                }
                                // Update
                                else
                                {
                                    detail.Quantity = podView.QuantityToOrder;
                                    detail.PurchasePrice = podView.PurchasePrice;
                                    _context.PurchaseOrderDetails.Update(detail);
                                }
                            }
                        }

                        // This is for adding additional purchase order detail
                        foreach (var item in added)
                        {
                            PurchaseOrderDetailView podView = purchaseOrder.PurchaseOrderDetails.FirstOrDefault(pod => pod.PartID==item);

                            if (podView.QuantityToOrder <= 0)
                            {
                                errorList.Add(new Exception($"Part {podView.Description} quantity to order must be greater than zero."));
                            }

                            if (podView.PurchasePrice <= 0)
                            {
                                errorList.Add(new Exception($"Part {podView.Description} price must be greater than zero."));
                            }

                            if (errorList.Count == 0)
                            {
                                PurchaseOrderDetail detailToAdd = new PurchaseOrderDetail();
                                detailToAdd.PurchaseOrderID = purchaseOrder.PurchaseOrderID;
                                detailToAdd.PartID = item;
                                detailToAdd.Quantity = podView.QuantityToOrder;
                                detailToAdd.PurchasePrice = podView.PurchasePrice;
                                _context.PurchaseOrderDetails.Add(detailToAdd);
                            }
                        }
                    }

                    // Get existing purchase order
                    existingPO = _context.PurchaseOrders
                        .Where(po => po.PurchaseOrderID == purchaseOrder.PurchaseOrderID)
                        .Select(po => po).FirstOrDefault();

                    if (existingPO != null)
                    {
                        existingPO.OrderDate = purchaseOrder.OrderDate;
                        existingPO.EmployeeID = purchaseOrder.EmployeeID;
                        existingPO.SubTotal = purchaseOrderToSave.PurchaseOrderDetails
                                            .Sum(pod => pod.Quantity * pod.PurchasePrice);
                        existingPO.TaxAmount = purchaseOrderToSave.SubTotal * 0.05m;
                        _context.PurchaseOrders.Update(existingPO);
                    }
                }
                // For suggested order
                else
                {
                    foreach (var purchaseDetail in purchaseOrder.PurchaseOrderDetails)
                    {
                        if (purchaseDetail.QuantityToOrder <= 0)
                        {
                            errorList.Add(new Exception($"Part {purchaseDetail.Description} quantity to order must be greater than zero."));
                        }

                        if (purchaseDetail.PurchasePrice <= 0)
                        {
                            errorList.Add(new Exception($"Part {purchaseDetail.Description} price must be greater than zero."));
                        }

                        if (errorList.Count == 0)
                        {
                            PurchaseOrderDetail purchaseOrderDetailToSave = new PurchaseOrderDetail();
                            purchaseOrderDetailToSave.PurchaseOrderID = purchaseOrder.PurchaseOrderID;
                            purchaseOrderDetailToSave.PartID = purchaseDetail.PartID;
                            purchaseOrderDetailToSave.Quantity = purchaseDetail.QuantityToOrder;
                            purchaseOrderDetailToSave.PurchasePrice = purchaseDetail.PurchasePrice;
                            purchaseOrderToSave.PurchaseOrderDetails.Add(purchaseOrderDetailToSave);
                        }
                    }

                    purchaseOrderToSave.PurchaseOrderNumber = _context.PurchaseOrders.Max(pod => pod.PurchaseOrderNumber) + 1;
                    purchaseOrderToSave.SubTotal = purchaseOrderToSave.PurchaseOrderDetails
                                                    .Sum(pod => pod.Quantity * pod.PurchasePrice);
                    purchaseOrderToSave.TaxAmount = purchaseOrderToSave.SubTotal * 0.05m;
                    purchaseOrderToSave.EmployeeID = purchaseOrder.EmployeeID;
                    purchaseOrderToSave.VendorID = purchaseOrder.VendorID;
                    _context.PurchaseOrders.Add(purchaseOrderToSave);
                }
            }

            // When order date is not null, it is a place order
            if (purchaseOrder.OrderDate != null)
            {
                // Update the quantity on order for each parts
                foreach (var purchaseOrderDetail in purchaseOrder.PurchaseOrderDetails)
                {
                    Part part = _context.Parts
                                    .Where(p => p.PartID == purchaseOrderDetail.PartID)
                                    .Select(p => p).FirstOrDefault();
                    part.QuantityOnOrder = part.QuantityOnOrder + purchaseOrderDetail.QuantityToOrder;
                    _context.Parts.Update(part);
                }
            }

            if (errorList.Count > 0)
            {
                throw new AggregateException("Unable to process the order. Check concerns", errorList.OrderBy(x => x.Message).ToList());
            }
            else
            {
                _context.SaveChanges();
            }
        }

        public void DeletePurchaseOrder(int purchaseOrderID)
        {
            List<Exception> errorList = new List<Exception>();

            if (purchaseOrderID == 0)
            {
                throw new ArgumentNullException("No Purchase Order ID provided.");
            }

            PurchaseOrder purchaseOrder = _context.PurchaseOrders
                .Where(po => po.PurchaseOrderID == purchaseOrderID)
                .Select(po => po).FirstOrDefault();

            if (purchaseOrder == null)
            {
                throw new Exception($"Purchase Order ID {purchaseOrderID} does not exist in the database.");
            }

            if (purchaseOrder.OrderDate != null)
            {
                throw new Exception($"Purhcase Order ID {purchaseOrderID} was already placed. A Placed Order cannot be deleted.");
            }

            if (errorList.Count == 0)
            {
                List<PurchaseOrderDetail> purchaseOrderDetails = _context.PurchaseOrderDetails
                                                                .Where(pod => pod.PurchaseOrderID == purchaseOrderID)
                                                                .Select(pod => pod).ToList();
                foreach (var purchaseOrderDetail in purchaseOrderDetails)
                {
                    _context.PurchaseOrderDetails.Remove(purchaseOrderDetail);
                }

                _context.PurchaseOrders.Remove(purchaseOrder);
            }

            if (errorList.Count > 0)
            {
                throw new AggregateException("Unable to process the order. Check concerns", errorList.OrderBy(x => x.Message).ToList());
            }
            else
            {
                _context.SaveChanges();
            }
        }
    }
}
