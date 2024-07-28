﻿// <auto-generated> This file has been auto generated by EF Core Power Tools. </auto-generated>
#nullable disable
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace PurchasingSystem.Entities
{
    public partial class Part
    {
        public Part()
        {
            PurchaseOrderDetails = new HashSet<PurchaseOrderDetail>();
        }

        [Key]
        public int PartID { get; set; }
        [Required]
        [StringLength(40)]
        [Unicode(false)]
        public string Description { get; set; }
        [Column(TypeName = "smallmoney")]
        public decimal PurchasePrice { get; set; }
        [Column(TypeName = "smallmoney")]
        public decimal SellingPrice { get; set; }
        public int QuantityOnHand { get; set; }
        public int ReorderLevel { get; set; }
        public int QuantityOnOrder { get; set; }
        public int CategoryID { get; set; }
        [Required]
        [StringLength(1)]
        [Unicode(false)]
        public string Refundable { get; set; }
        public bool Discontinued { get; set; }
        public int VendorID { get; set; }

        [ForeignKey("CategoryID")]
        [InverseProperty("Parts")]
        public virtual Category Category { get; set; }
        [ForeignKey("VendorID")]
        [InverseProperty("Parts")]
        public virtual Vendor Vendor { get; set; }
        [InverseProperty("Part")]
        public virtual ICollection<PurchaseOrderDetail> PurchaseOrderDetails { get; set; }
    }
}