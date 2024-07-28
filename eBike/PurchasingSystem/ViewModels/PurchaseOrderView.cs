using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PurchasingSystem.ViewModels
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
        public string? Notes { get; set; }
        public List<PurchaseOrderDetailView>? PurchaseOrderDetails { get; set; }
    }
}
