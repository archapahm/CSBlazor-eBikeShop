﻿// <auto-generated> This file has been auto generated by EF Core Power Tools. </auto-generated>
#nullable disable
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace ServicingSystem.Entities
{
    public partial class JobDetail
    {
        public JobDetail()
        {
            JobDetailParts = new HashSet<JobDetailPart>();
        }

        [Key]
        public int JobDetailID { get; set; }
        public int JobID { get; set; }
        [Required]
        [StringLength(100)]
        public string Description { get; set; }
        [Column(TypeName = "decimal(5, 2)")]
        public decimal JobHours { get; set; }
        public string Comments { get; set; }
        public int? CouponID { get; set; }
        [Required]
        [StringLength(1)]
        [Unicode(false)]
        public string StatusCode { get; set; }
        public int? EmployeeID { get; set; }

        [ForeignKey("CouponID")]
        [InverseProperty("JobDetails")]
        public virtual Coupon Coupon { get; set; }
        [ForeignKey("EmployeeID")]
        [InverseProperty("JobDetails")]
        public virtual Employee Employee { get; set; }
        [ForeignKey("JobID")]
        [InverseProperty("JobDetails")]
        public virtual Job Job { get; set; }
        [InverseProperty("JobDetail")]
        public virtual ICollection<JobDetailPart> JobDetailParts { get; set; }
    }
}