using System.ComponentModel.DataAnnotations;

namespace ViewModels
{
   public class RegisterViewModel
   {
      [Required]
      [MinLength(2)]
      [Display(Name = "Full Name")]
      public string Name { get; set; }

      [Required]
      [MinLength(3)]
      [Display(Name = "Username")]
      public string UserName { get; set; }

      [Required]
      [DataType(DataType.EmailAddress)]
      [Display(Name = "Email Address")]
      public string Email { get; set; }

      [Required]
      [DataType(DataType.Password)]
      public string Password { get; set; }

      [Required]
      [Display(Name = "Confirm Password")]
      [DataType(DataType.Password)]
      [Compare("Password")]
      public string ConfirmPassword { get; set; }
   }
}