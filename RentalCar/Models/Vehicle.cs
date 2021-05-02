using System.Collections.Generic;
using Common;

namespace Models
{
   public class Vehicle
   {
      public int Id { get; set; }
      public string Mark { get; set; }
      public string Name { get; set; }
      public int Seats { get; set; }
      public int HorsePower { get; set; }
      public VehicleTransmission Transmission { get; set; }
      public string Description { get; set; }
      public string Image { get; set; }
      public double Price { get; set; }

      public List<Reservation> Reservations { get; set; } = new List<Reservation>();
      public List<Rating> Ratings { get; set; } = new List<Rating>();
   }
}