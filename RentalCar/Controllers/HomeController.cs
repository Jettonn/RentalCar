using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Common;
using Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Models;
using RentalCar.Models;
using ViewModels;

namespace RentalCar.Controllers
{
   [AllowAnonymous]
   public class HomeController : BaseController
   {
      private readonly DataContext _context;
      private readonly UserManager<AppUser> _userManager;

      public HomeController(DataContext context, UserManager<AppUser> userManager)
      {
         this._userManager = userManager;
         this._context = context;
      }

      public IActionResult Index()
      {
         return View();
      }

      [Authorize]
      [HttpGet("/")]
      public async Task<IActionResult> Main([FromQuery] MainViewModelFilters filters)
      {
         // If not admin, filter only reservations of this user
         var user = await _userManager.GetUserAsync(HttpContext.User);
         var isAdmin = await _userManager.IsInRoleAsync(user, "Admin");

         int finishedRentals;
         if (isAdmin)
         {
            finishedRentals = await _context.Reservations
            .Where(r => r.ReservedTo.CompareTo(DateTime.UtcNow) < 0)
            .CountAsync();
         }
         else
         {
            finishedRentals = await _context.Reservations
           .Where(r => r.ReservedTo.CompareTo(DateTime.UtcNow) < 0 &&
               r.UserId == user.Id)
           .CountAsync();
         }

         int onGoingRentals;
         if (isAdmin)
         {
            onGoingRentals = await _context.Reservations
           .Where(r => r.ReservedFrom.CompareTo(DateTime.UtcNow) < 0 &&
              r.ReservedTo.CompareTo(DateTime.UtcNow) > 0)
           .CountAsync();
         }
         else
         {
            onGoingRentals = await _context.Reservations
           .Where(r => r.ReservedFrom.CompareTo(DateTime.UtcNow) < 0 &&
              r.ReservedTo.CompareTo(DateTime.UtcNow) > 0 &&
               r.UserId == user.Id)
           .CountAsync();
         }

         int upcomingRentals;
         if (isAdmin)
         {
            upcomingRentals = await _context.Reservations
            .Where(r => r.ReservedFrom.CompareTo(DateTime.UtcNow) > 0)
            .CountAsync();
         }
         else
         {
            upcomingRentals = await _context.Reservations
               .Where(r => r.ReservedFrom.CompareTo(DateTime.UtcNow) > 0 &&
                  r.UserId == user.Id)
               .CountAsync();
         }

         double totalIncome = 0;
         if (isAdmin)
         {
            var allReservations = await _context.Reservations
                        .Include(r => r.Vehicle)
                        .ToListAsync();

            totalIncome = allReservations.Sum(r => r.TotalAmount);
         }


         var vehicles = _context.Vehicles.AsQueryable();

         if (!string.IsNullOrEmpty(filters.Mark))
         {
            vehicles = vehicles.Where(v => v.Mark.Contains(filters.Mark));
         }

         if (filters.Seats >= 1 && filters.Seats <= 6)
         {
            vehicles = vehicles.Where(v => v.Seats == filters.Seats);
         }

         if (filters.Transmission >= 0 && filters.Transmission <= 3)
         {
            vehicles = vehicles.Where(v => (int)v.Transmission == filters.Transmission);
         }

         if (filters.PriceFrom > 0)
         {
            vehicles = vehicles.Where(v => v.Price >= filters.PriceFrom);
         }

         if (filters.PriceTo > 0)
         {
            vehicles = vehicles.Where(v => v.Price <= filters.PriceTo);
         }

         var viewModel = new MainViewModel
         {
            FinishedRentalsCount = finishedRentals,
            OnGoingRentalsCount = onGoingRentals,
            UpcomingRentalsCount = upcomingRentals,
            Vehicles = await vehicles.ToListAsync(),
            TotalAmount = totalIncome,
         };

         return View(viewModel);
      }

   }
}
