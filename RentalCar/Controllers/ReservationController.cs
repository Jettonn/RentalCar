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
   [Authorize]
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
      public async Task<IActionResult> Index()
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
      public async Task<IActionResult> Create()
      {
         var vehicles = await _context.Vehicles.ToListAsync();
         ViewBag.Vehicles = vehicles;
         return View();
      }

      [HttpPost]
      public async Task<IActionResult> Create(ReservationViewModel reservationViewModel)
      {
         if (ModelState.IsValid)
         {
            // ReservedTo is before ReservedFrom
            if (reservationViewModel.ReservedFrom.ToUniversalTime()
               .CompareTo(reservationViewModel.ReservedTo.ToUniversalTime()) > 0)
            {
               ModelState.AddModelError(string.Empty, "Life can only be understood backwards; but it must be lived forwards. (i.e pick a future date for the reservation)");

               var vehicles = await _context.Vehicles.ToListAsync();
               ViewBag.Vehicles = vehicles;
               return View();
            }

            // Get latest reservation
            var latestReservation = await _context.Reservations
                  .OrderByDescending(r => r.Id)
                  .Take(1)
                  .FirstAsync();

            // Check if dates collide
            if (reservationViewModel.ReservedFrom.ToUniversalTime()
                .CompareTo(latestReservation.ReservedTo) < 0)
            {
               ModelState.AddModelError(string.Empty, $"This vehicle is already reserved. Please specify a date later than '{latestReservation.ReservedTo.ToUniversalTime()}'");

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
      public async Task<IActionResult> Edit(int reservationId, ReservationEditViewModel reservationViewModel)
      {
         ModelState.Remove("VehicleId");

         if (ModelState.IsValid)
         {
            var reservation = await _context.Reservations
                .FirstOrDefaultAsync(r => r.Id == reservationId);

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
               return RedirectToAction("Details", new { reservationId = reservationId });
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