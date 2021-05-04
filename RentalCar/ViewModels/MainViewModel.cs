using System.Collections.Generic;
using Common;
using Models;

namespace ViewModels
{
   public class MainViewModel
   {
      public int FinishedRentalsCount { get; set; }
      public int OnGoingRentalsCount { get; set; }
      public int UpcomingRentalsCount { get; set; }
      public List<Vehicle> Vehicles { get; set; }
      public double TotalAmount { get; set; }
      public MainViewModelFilters Filters { get; set; }
   }
}