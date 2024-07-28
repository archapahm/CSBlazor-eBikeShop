﻿// <auto-generated> This file has been auto generated by EF Core Power Tools. </auto-generated>
#nullable disable
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace PurchasingSystem.Entities
{
    public partial class Category
    {
        public Category()
        {
            Parts = new HashSet<Part>();
        }

        [Key]
        public int CategoryID { get; set; }
        [Required]
        [StringLength(40)]
        [Unicode(false)]
        public string Description { get; set; }

        [InverseProperty("Category")]
        public virtual ICollection<Part> Parts { get; set; }
    }
}