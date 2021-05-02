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
   public class VehicleController : Controller
   {
      private readonly DataContext _context;
      public VehicleController(DataContext context)
      {
         this._context = context;
      }

      [HttpGet]
      public async Task<IActionResult> Index()
      {
         var vehicles = await _context.Vehicles
           .ToListAsync();

         return View(vehicles);
      }

      [HttpGet("{vehicleId}")]
      public async Task<IActionResult> Details(int vehicleId)
      {
         var vehicle = await _context.Vehicles
            .Include(v => v.Ratings)
               .ThenInclude(r => r.User)
            .FirstOrDefaultAsync(v => v.Id == vehicleId);

         if (vehicle is null)
         {
            return RedirectToAction("Index");
         }

         return View(vehicle);
      }

      [HttpPost]
      [Authorize(Roles = "Admin")]
      public async Task<IActionResult> Create(VehicleViewModel vehicleViewModel)
      {
         if (ModelState.IsValid)
         {
            var vehicle = new Vehicle
            {
               Mark = vehicleViewModel.Mark,
               Name = vehicleViewModel.Name,
               Seats = vehicleViewModel.Seats,
               HorsePower = vehicleViewModel.HorsePower,
               Transmission = vehicleViewModel.Transmission,
               Description = vehicleViewModel.Description,
               Image = vehicleViewModel.Image,
               Price = vehicleViewModel.Price,
            };

            _context.Vehicles.Add(vehicle);
            var success = await _context.SaveChangesAsync() > 0;

            if (success)
            {
               return RedirectToAction("Index");
            }
         }

         return View();
      }

      [HttpPost("{vehicleId}")]
      [Authorize(Roles = "Admin")]
      public async Task<IActionResult> Edit(int vehicleId, VehicleViewModel vehicleViewModel)
      {
         if (ModelState.IsValid)
         {
            var vehicle = await _context.Vehicles
                .FirstOrDefaultAsync(v => v.Id == vehicleId);

            if (vehicle is null)
            {
               return RedirectToAction("Index");
            }

            vehicle.Mark = vehicleViewModel.Mark;
            vehicle.Name = vehicleViewModel.Name;
            vehicle.Seats = vehicleViewModel.Seats;
            vehicle.HorsePower = vehicleViewModel.HorsePower;
            vehicle.Transmission = vehicleViewModel.Transmission;
            vehicle.Description = vehicleViewModel.Description;
            vehicle.Image = vehicleViewModel.Image;
            vehicle.Price = vehicleViewModel.Price;

            var success = await _context.SaveChangesAsync() > 0;

            if (success)
            {
               return RedirectToAction("Details", new { vehicleId = vehicleId });
            }
         }

         return View();
      }

      [HttpPost("{vehicleId}")]
      [Authorize(Roles = "Admin")]
      public async Task<IActionResult> Delete(int vehicleId)
      {
         var vehicle = await _context.Vehicles
             .FirstOrDefaultAsync(v => v.Id == vehicleId);

         if (vehicle is null)
         {
            return RedirectToAction("Index");
         }

         _context.Vehicles.Remove(vehicle);
         var success = await _context.SaveChangesAsync() > 0;

         if (success)
         {
            return RedirectToAction("Index");
         }

         return View();
      }
   }
}