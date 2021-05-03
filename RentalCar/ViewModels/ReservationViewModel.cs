using System;
using System.ComponentModel.DataAnnotations;
using Models;

namespace ViewModels
{
   public class ReservationViewModel
   {
      [Required]
      public DateTime ReservedFrom { get; set; }

      [Required]
      public DateTime ReservedTo { get; set; }

      [Required]
      public int VehicleId { get; set; }

      [Required]
      public string UserId { get; set; }

      public string DeliveryAddress { get; set; }

      [Required]
      [Range(1, 365)]
      public int NumberOfDays { get; set; }
   }
}