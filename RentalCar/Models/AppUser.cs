using System;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace Models
{
   public class AppUser : IdentityUser
   {
      [Required]
      [MinLength(2)]
      public string Name { get; set; }

      public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
   }
}