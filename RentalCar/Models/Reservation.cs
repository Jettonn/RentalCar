using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace Models
{
   public class Reservation
   {
      public int Id { get; set; }
      public DateTime ReservedFrom { get; set; }
      public DateTime ReservedTo { get; set; }
      public int VehicleId { get; set; }
      public Vehicle Vehicle { get; set; }

      public string UserId { get; set; }
      public AppUser User { get; set; }
      public string DeliveryAddress { get; set; }
      public int NumberOfDays { get; set; }

      [NotMapped]
      public double TotalAmount
      {
         get
         {
            if (Vehicle is null)
            {
               return 0;
            }
            else
            {
               return NumberOfDays * Vehicle.Price;
            }
         }
      }
   }
}