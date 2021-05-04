using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Models;
using ViewModels;

namespace RentalCar.Controllers
{
   public class ReservationController : BaseController
   {
      private readonly UserManager<AppUser> _userManager;

      public ReservationController(DataContext context, UserManager<AppUser> userManager)
      {
         this._context = context;
         this._userManager = userManager;
      }
      private readonly DataContext _context;

      [HttpGet]
      public async Task<IActionResult> Index(bool finished, bool ongoing, bool upcoming)
      {
         var currentUserId = HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
         var user = await _userManager.FindByIdAsync(currentUserId);
         if (user is null)
         {
            return RedirectToAction("Main", "Home");
         }

         var reservations = _context.Reservations.AsQueryable();

         // If not admin, filter only reservations of this user
         if (!(await _userManager.IsInRoleAsync(user, "Admin")))
         {
            reservations = reservations
                           .Where(r => r.UserId == user.Id);
         }

         reservations = reservations
                       .Include(r => r.User)
                       .Include(r => r.Vehicle);

         if (finished)
         {
            reservations = reservations.Where(r => r.ReservedTo.CompareTo(DateTime.UtcNow) < 0);
         }
         else if (ongoing)
         {
            reservations = reservations.Where(r => r.ReservedFrom.CompareTo(DateTime.UtcNow) < 0 &&
               r.ReservedTo.CompareTo(DateTime.UtcNow) > 0);
         }
         else if (upcoming)
         {
            reservations = reservations.Where(r => r.ReservedFrom.CompareTo(DateTime.UtcNow) > 0);
         }

         return View(await reservations.ToListAsync());
      }

      [HttpGet("{reservationId}")]
      public async Task<IActionResult> Details(int reservationId)
      {
         var reservation = await _context.Reservations
            .Include(r => r.User)
            .Include(r => r.Vehicle)
            .FirstOrDefaultAsync(r => r.Id == reservationId);

         if (reservation is null)
         {
            return RedirectToAction("Index");
         }

         return View(reservation);
      }

      [HttpGet]
      public async Task<IActionResult> Create(int vehicleId)
      {
         var vehicles = await _context.Vehicles.ToListAsync();
         ViewBag.Vehicles = vehicles;
         ViewBag.VehicleId = vehicleId;
         return View();
      }

      [HttpPost]
      public async Task<IActionResult> Create(ReservationViewModel reservationViewModel)
      {
         if (ModelState.IsValid)
         {
            // Validate if ReservedFrom and ReservedTo are future dates and ReservedTo is not earlier than ReservedFrom
            if (reservationViewModel.ReservedFrom.ToUniversalTime()
                    .CompareTo(reservationViewModel.ReservedTo.ToUniversalTime()) > 0 ||
               reservationViewModel.ReservedFrom.ToUniversalTime()
                    .CompareTo(DateTime.UtcNow) < 0 ||
               reservationViewModel.ReservedTo.ToUniversalTime()
                    .CompareTo(reservationViewModel.ReservedTo.ToUniversalTime()) < 0)
            {
               ModelState.AddModelError(string.Empty, "Invalid date selected. Please pick a valid date range.");

               var vehicles = await _context.Vehicles.ToListAsync();
               ViewBag.Vehicles = vehicles;
               return View();
            }

            if (await IsTimeConflicting(reservationViewModel))
            {
               ModelState.AddModelError(string.Empty,
                    $"This vehicle is already reserved for these dates. Please change the dates and retry again.");

               var vehicles = await _context.Vehicles.ToListAsync();
               ViewBag.Vehicles = vehicles;
               return View();
            }

            var reservation = new Reservation
            {
               ReservedFrom = reservationViewModel.ReservedFrom.ToUniversalTime(),
               ReservedTo = reservationViewModel.ReservedTo.ToUniversalTime(),
               VehicleId = reservationViewModel.VehicleId,
               UserId = reservationViewModel.UserId,
               DeliveryAddress = reservationViewModel.DeliveryAddress,
               NumberOfDays = (reservationViewModel.ReservedTo.ToUniversalTime().Date - reservationViewModel.ReservedFrom.ToUniversalTime().Date).Days > 0 ? (reservationViewModel.ReservedTo.ToUniversalTime().Date - reservationViewModel.ReservedFrom.ToUniversalTime().Date).Days : 1,
            };

            _context.Reservations.Add(reservation);
            var success = await _context.SaveChangesAsync() > 0;

            if (success)
            {
               return RedirectToAction("Index");
            }
         }

         return View();
      }
      [NonAction]
      private async Task<bool> IsTimeConflicting(ReservationViewModel reservation)
      {
         var reservationSchedule = await _context
            .Reservations.Where(r => r.UserId == reservation.UserId)
            .ToListAsync();

         // Check if start time is in range of any other reservation
         if (reservationSchedule.Any(r => r.ReservedFrom.CompareTo(reservation.ReservedFrom) < 0 &&
             r.ReservedTo.CompareTo(reservation.ReservedFrom) > 0))
         {
            return true;
         }

         // Check if end time is in range of any other reservation
         if (reservationSchedule.Any(r => r.ReservedFrom.CompareTo(reservation.ReservedTo) < 0 &&
             r.ReservedTo.CompareTo(reservation.ReservedTo) > 0))
         {
            return true;
         }

         return false;
      }

      [HttpGet("/EditReservation/{reservationId}")]
      public async Task<IActionResult> Edit(int reservationId)
      {
         var reservationInfo = await _context.Reservations
            .Include(r => r.User)
            .Include(r => r.Vehicle)
            .FirstOrDefaultAsync(r => r.Id == reservationId);

         var vehicles = await _context.Vehicles.ToListAsync();
         ViewBag.Vehicles = vehicles;

         if (reservationInfo is null)
         {
            return RedirectToAction("Index");
         }

         return View(reservationInfo);
      }


      [Authorize(Roles = "Admin")]
      [HttpPost("{reservationId}")]
      public async Task<IActionResult> Edit(Reservation reservationViewModel)
      {
         ModelState.Remove("VehicleId");

         if (ModelState.IsValid)
         {
            var reservation = await _context.Reservations
                .FirstOrDefaultAsync(r => r.Id == reservationViewModel.Id);

            if (reservation is null)
            {
               return RedirectToAction("Index");
            }

            if (reservation.ReservedTo.ToUniversalTime().CompareTo(DateTime.UtcNow) > 0)
            {
               reservation.ReservedTo = reservationViewModel.ReservedTo;
            }

            if (!string.IsNullOrEmpty(reservationViewModel.DeliveryAddress))
            {
               reservation.DeliveryAddress = reservationViewModel.DeliveryAddress;
            }

            if (reservationViewModel.NumberOfDays > 0)
            {
               reservation.NumberOfDays = reservationViewModel.NumberOfDays;
            }

            var success = await _context.SaveChangesAsync() > 0;

            if (success)
            {
               return RedirectToAction("Details", new { reservationId = reservationViewModel.Id });
            }
         }

         return View();
      }

      [Authorize(Roles = "Admin")]
      [HttpPost()]
      public async Task<IActionResult> Delete(int reservationId)
      {
         var reservation = await _context.Reservations
             .FirstOrDefaultAsync(r => r.Id == reservationId);

         if (reservation is null)
         {
            return RedirectToAction("Index");
         }

         _context.Reservations.Remove(reservation);
         var success = await _context.SaveChangesAsync() > 0;

         if (success)
         {
            return RedirectToAction("Index");
         }

         return View();
      }
   }
}