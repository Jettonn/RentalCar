namespace Common
{
   public class MainViewModelFilters
   {
      public string Mark { get; set; }
      public int Seats { get; set; }
      public int Transmission { get; set; } = -1;
      public double PriceFrom { get; set; }
      public double PriceTo { get; set; }
   }
}