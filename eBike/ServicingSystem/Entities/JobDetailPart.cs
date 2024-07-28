﻿// <auto-generated> This file has been auto generated by EF Core Power Tools. </auto-generated>
#nullable disable
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace ServicingSystem.Entities
{
    public partial class JobDetailPart
    {
        [Key]
        public int JobDetailPartID { get; set; }
        public int JobDetailID { get; set; }
        public int PartID { get; set; }
        public short Quantity { get; set; }
        [Column(TypeName = "smallmoney")]
        public decimal SellingPrice { get; set; }

        [ForeignKey("JobDetailID")]
        [InverseProperty("JobDetailParts")]
        public virtual JobDetail JobDetail { get; set; }
        [ForeignKey("PartID")]
        [InverseProperty("JobDetailParts")]
        public virtual Part Part { get; set; }
    }
}