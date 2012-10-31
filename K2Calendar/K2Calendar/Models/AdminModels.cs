using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Web.Mvc;

namespace K2Calendar.Models
{
    public class UserInfoSummaryModel
    {
        [DataType(DataType.Text)]
        [Display(Name = "Username")]
        public string Username { get; set; }

        [DataType(DataType.Text)]
        [Display(Name = "Roles")]
        public string[] Roles { get; set; }

        [DataType(DataType.Text)]
        [Display(Name = "First name")]
        public string FirstName { get; set; }

        [DataType(DataType.Text)]
        [Display(Name = "Last name")]
        public string LastName { get; set; }

        [DataType(DataType.Text)]
        [Display(Name = "Email")]
        public string Email { get; set; }

        [DataType(DataType.DateTime)]
        [Display(Name = "Last login")]
        public DateTime LastLogin { get; set; }

        public int UserId { get; set; }

        public string MembershipId { get; set; }

    }
}
