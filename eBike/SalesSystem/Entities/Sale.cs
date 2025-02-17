﻿// <auto-generated> This file has been auto generated by EF Core Power Tools. </auto-generated>
#nullable disable
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace SalesSystem.Entities
{
    internal partial class Sale
    {
        public Sale()
        {
            SaleDetails = new HashSet<SaleDetail>();
            SaleRefunds = new HashSet<SaleRefund>();
        }

        [Key]
        public int SaleID { get; set; }
        [Column(TypeName = "datetime")]
        public DateTime SaleDate { get; set; }
        public int EmployeeID { get; set; }
        [Column(TypeName = "money")]
        public decimal TaxAmount { get; set; }
        [Column(TypeName = "money")]
        public decimal SubTotal { get; set; }
        public int? CouponID { get; set; }
        [Required]
        [StringLength(1)]
        public string PaymentType { get; set; }

        [ForeignKey("CouponID")]
        [InverseProperty("Sales")]
        public virtual Coupon Coupon { get; set; }
        [ForeignKey("EmployeeID")]
        [InverseProperty("Sales")]
        public virtual Employee Employee { get; set; }
        [InverseProperty("Sale")]
        public virtual ICollection<SaleDetail> SaleDetails { get; set; }
        [InverseProperty("Sale")]
        public virtual ICollection<SaleRefund> SaleRefunds { get; set; }
    }
}