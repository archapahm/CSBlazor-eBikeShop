using System;
using System.Collections.Generic;

namespace eBike
{
    public class PurchaseOrderView
    {
        public int PurchaseOrderID { get; set; }
        public int PurchaseOrderNumber { get; set; }
        public int EmployeeID { get; set; }
        public int VendorID { get; set; }
        public DateTime? OrderDate { get; set; }
        public decimal SubTotal { get; set; }
        public decimal TaxAmount { get; set; }
        public string Notes { get; set; }
        public List<PurchaseOrderDetailView> PurchaseOrderDetails { get; set; }
    }

    public class PurchaseOrderDetailView
    {
        public int PurchaseOrderDetailID { get; set; }
        public int PartID { get; set; }
        public string Description { get; set; }
        public int QuantityOnHand { get; set; }
        public int ReorderLevel { get; set; }
        public int QuantityOnOrder { get; set; }
        public int QuantityToOrder { get; set; }
        public decimal PurchasePrice { get; set; }
    }
}