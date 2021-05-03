namespace Models
{
   public class Rating
   {
      public int Id { get; set; }
      public string UserId { get; set; }
      public AppUser User { get; set; }
      public int VehicleId { get; set; }
      public Vehicle Vehicle { get; set; }
      public int RatingScore { get; set; }
      public string Comment { get; set; }
   }
}