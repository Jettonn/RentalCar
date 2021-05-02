using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Common;
using Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using RentalCar.Models;

namespace RentalCar.Controllers
{
   public class HomeController : Controller
   {
      private readonly DataContext _context;

      public HomeController(DataContext context)
      {
         this._context = context;
      }

      public IActionResult Index()
      {
         return View();
      }

      [Authorize]
      public IActionResult Main()
      {
         return View();
      }

      [Authorize]
      public async Task<IActionResult> Filter(string mark, int seats, int transmission, double priceFrom, double priceTo)
      {
         var vehicles = _context.Vehicles.AsQueryable();

         if (!string.IsNullOrEmpty(mark))
         {
            vehicles = vehicles.Where(v => v.Mark.Contains(mark));
         }

         if (seats >= 1 && seats <= 6)
         {
            vehicles = vehicles.Where(v => v.Seats == seats);
         }

         if (transmission >= 0 && transmission <= 3)
         {
            vehicles = vehicles.Where(v => (int)v.Transmission == transmission);
         }

         if (priceFrom > 0 && priceTo > priceFrom)
         {
            vehicles = vehicles.Where(v => v.Price >= priceFrom && v.Price <= priceTo);
         }

         return View(await vehicles.ToListAsync());
      }
   }
}
