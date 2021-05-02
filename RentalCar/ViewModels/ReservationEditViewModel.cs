using System;
using System.ComponentModel.DataAnnotations;

namespace ViewModels
{
   public class ReservationEditViewModel
   {
      public DateTime ReservedTo { get; set; }
      public string DeliveryAddress { get; set; }

      [Range(1, 365)]
      public int NumberOfDays { get; set; }
   }
}