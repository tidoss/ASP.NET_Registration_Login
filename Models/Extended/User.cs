using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace INFMDOTNET.Models
{
    [MetadataType(typeof(UserMetadata))]
    public partial class User
    {
        public string ConfirmPassword { get; set; }
    }

    public class UserMetadata
    {
        [Display(Name = "First Name")]
        [Required(AllowEmptyStrings = false, ErrorMessage = "First Name is required")]
        public string FirstName { get; set; }

        [Display(Name = "Last Name")]
        [Required(AllowEmptyStrings = false, ErrorMessage = "Last Neme is required")]
        public string LastName { get; set; }

        [Display(Name = "Email ID")]
        [Required(AllowEmptyStrings = false, ErrorMessage = "Email ID is required")]
        [DataType(DataType.EmailAddress)]
        public string EmailID { get; set; }

        [Display(Name = "Date Of Birth")]
        [DataType(DataType.Date)]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:MM/dd/yyyy}")]
        public DateTime DateOfBirth { get; set; }

        [Required(AllowEmptyStrings = false, ErrorMessage = "Password ID is required")]
        [DataType(DataType.Password)]
        [MinLength(6, ErrorMessage = "Minimun 6 characters are required")]
        public string Password { get; set; }

        [Display(Name = "Confirm Password")]
        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = "Please add a corrent password for confirmation")]
        public string ConfirmPassword { get; set; }
    }
}