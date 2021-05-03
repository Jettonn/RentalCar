using System.ComponentModel.DataAnnotations;
using Common;

namespace ViewModels
{
   public class VehicleViewModel
   {
      [Required]
      public string Mark { get; set; }
      
      [Required]
      public string Name { get; set; }

      [Required]
      [Range(1, 6)]
      public int Seats { get; set; }

      [Range(1, int.MaxValue)]
      public int HorsePower { get; set; }

      [Required]
      [Range(1, 4)]
      public VehicleTransmission Transmission { get; set; }

      public string Description { get; set; }

      [Required]
      public string Image { get; set; }

      [Required]
      [Range(1, double.MaxValue)]
      public double Price { get; set; }
   }
}