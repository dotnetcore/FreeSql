using System;
using System.ComponentModel.DataAnnotations;

namespace Densen.Identity.Models
{

    public class WebAppIdentityUser
    {


        /// <summary>
        /// Full name
        /// </summary>
        [Display(Name = "全名")]
        public string? Name { get; set; }

        /// <summary>
        /// Birth Date
        /// </summary>
        [Display(Name = "生日")]
        public DateTime? DOB { get; set; }

        [Display(Name = "识别码")]
        public string? UUID { get; set; }

        [Display(Name = "外联")]
        public string? provider { get; set; }

        [Display(Name = "税号")]
        public string? TaxNumber { get; set; }

        [Display(Name = "街道地址")]
        public string? Street { get; set; }

        [Display(Name = "邮编")]
        public string? Zip { get; set; }

        [Display(Name = "县")]
        public string? County { get; set; }

        [Display(Name = "城市")]
        public string? City { get; set; }

        [Display(Name = "省份")]
        public string? Province { get; set; }

        [Display(Name = "国家")]
        public string? Country { get; set; }

        [Display(Name = "类型")]
        public string? UserRole { get; set; }
    }
}