using System;
using System.Linq;
using System.Threading.Tasks;
using Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Models;
using ViewModels;

namespace Controllers
{
   [Authorize]
   public class ReservationController : Controller
   {
      private readonly DataContext _context;
      public ReservationController(DataContext context)
      {
         this._context = context;
      }

      [HttpGet]
      public async Task<IActionResult> Index()
      {
         var reservations = await _context.Reservations
            .Include(r => r.User)
            .Include(r => r.Vehicle)
            .ToListAsync();

         return View(reservations);
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

      [HttpPost]
      public async Task<IActionResult> Create(ReservationViewModel reservationViewModel)
      {
         if (ModelState.IsValid)
         {
            // Get latest reservation
            var latestReservation = await _context.Reservations
                  .OrderByDescending(r => r.Id)
                  .Take(1)
                  .FirstAsync();

            // Check if dates collide
            if (reservationViewModel.ReservedFrom.ToUniversalTime().CompareTo(latestReservation.ReservedTo.ToUniversalTime()) < 0)
            {
               ModelState.AddModelError(string.Empty, $"This vehicle is already reserved. Please specify a date later than '{latestReservation.ReservedTo.ToUniversalTime().ToShortDateString()}'");
               return View();
            }

            var reservation = new Reservation
            {
               ReservedFrom = reservationViewModel.ReservedFrom.ToUniversalTime(),
               ReservedTo = reservationViewModel.ReservedTo.ToUniversalTime(),
               VehicleId = reservationViewModel.VehicleId,
               UserId = reservationViewModel.UserId,
               DeliveryAddress = reservationViewModel.DeliveryAddress,
               NumberOfDays = reservationViewModel.NumberOfDays,
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
      [HttpPost("{reservationId}")]
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