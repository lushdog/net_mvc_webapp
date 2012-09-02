using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Web.Mvc;

namespace K2Calendar.Models
{

    public class ChangePasswordModel
    {
        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "Current password")]
        public string OldPassword { get; set; }

        [Required]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "New password")]
        public string NewPassword { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Confirm new password")]
        [Compare("NewPassword", ErrorMessage = "The new password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; }
    }

    public class LogOnModel
    {
        [Required]
        [Display(Name = "User name")]
        public string UserName { get; set; }

        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; }

        [Display(Name = "Remember me?")]
        public bool RememberMe { get; set; }
    }

    public class UserInfoModel
    {
        [Key]
        [Required]
        public int Id { get; set; }
        
        /// <summary>
        /// <para>Corresponds to ProviderUserKey in ASP Membership</para>
        /// <para>Used in Membership.GetUser(ProviderUserKey) to retrieve email and username.</para>
        /// </summary>
        public Guid MembershipId { get; set; }

        [Required]
        [DataType(DataType.Text)]
        [Display(Name = "First name")]
        public string FirstName { get; set; }

        [Required]
        [DataType(DataType.Text)]
        [Display(Name = "Last name")]
        public string LastName { get; set; }

        [Required]
        [DataType(DataType.Text)]
        [Display(Name = "Address 1")]
        public string Address1 { get; set; }

        [DataType(DataType.Text)]
        [Display(Name = "Address 2")]
        public string Address2 { get; set; }

        [Required]
        [DataType(DataType.Text)]
        [Display(Name = "State")]
        public string State { get; set; }

        [Required]
        [DataType(DataType.Text)]
        [Display(Name = "Country")]
        public string Country { get; set; }

        [Required]
        [DataType(DataType.Text)]
        [Display(Name = "Zip code")]
        public string ZipCode { get; set; }

        [Required]
        [DataType(DataType.PhoneNumber)]
        [Display(Name = "Phone number")]
        public string PhoneNumber { get; set; }

        [Required]
        [DataType(DataType.Date)]
        [Display(Name = "Sign up date")]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = @"{0:MM/dd\/yyyy}")]
        public DateTime SignUpDate { get; set; }

        [Required]
        [DataType(DataType.Date)]
        [Display(Name = "Birthday")]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = @"{0:MM\/dd\/yyyy}")]
        public DateTime BirthDate { get; set; }

        [DataType(DataType.Date)]
        [Display(Name = "Enrollment date")]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = @"{0:MM\/dd\/yyyy}")]
        public DateTime EnrollmentDate { get; set; }
       
        [DataType(DataType.ImageUrl)]
        [Display(Name = "Avatar")]
        public string AvatarImage { get; set; }
        
        [Display(Name = "Rank")]
        public int RankId { get; set; }

        [ForeignKey("RankId")]
        public UserRankModel Rank { get; set; }

        [Display(Name = "IsActive")]
        public bool IsActive { get; set; }

    }
    
    public class CreateUserModel
    {
        [Required]
        [Display(Name = "User name")]
        public string UserName { get; set; }

        [Required]
        [DataType(DataType.EmailAddress)]
        [Display(Name = "Email")]
        public string Email { get; set; }

        [Required]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Confirm password")]
        [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; }

        public UserInfoModel UserInfoModel { get; set; }
    }

    public class UserEditInfoModel
    {
        [Editable(false)]
        [Display(Name = "User name")]
        public string UserName { get; set; }

        [Editable(false)]
        [Display(Name = "Email")]
        public string Email { get; set; }

        public UserInfoModel UserInfoModel { get; set; }
    }
    
    public class UserRankModel
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Required]
        public int Id { get; set; }

        [Required]
        [Display(Name="Name")]
        public string Name { get; set; }

        [Required]
        [Display(Name="Image")]
        public string Image { get; set; }

        [Required]
        [Display(Name="Rank level")]
        public int Level { get; set; }
    }
    
    
    /*
    public class RegisterModel
    {
        [Required]
        [Display(Name = "User name")]
        public string UserName { get; set; }

        [Required]
        [DataType(DataType.EmailAddress)]
        [Display(Name = "Email address")]
        public string Email { get; set; }

        [Required]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Confirm password")]
        [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; }
    }
    */
}
