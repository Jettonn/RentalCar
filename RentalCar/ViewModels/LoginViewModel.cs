using System.ComponentModel.DataAnnotations;

namespace ViewModels
{
   public class LoginViewModel
   {
      [Required]
      [DataType(DataType.EmailAddress)]
      [Display(Name = "Email Address")]
      public string Email { get; set; }

      [Required]
      [DataType(DataType.Password)]
      public string Password { get; set; }
   }
}