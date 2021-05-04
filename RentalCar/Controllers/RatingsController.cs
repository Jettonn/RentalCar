using System.Threading.Tasks;
using Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Models;
using ViewModels;

namespace RentalCar.Controllers
{
   public class RatingsController : BaseController
   {
      private readonly DataContext _context;

      public RatingsController(DataContext context)
      {
         this._context = context;
      }

      [HttpPost]
      public async Task<IActionResult> Create(RatingViewModel ratingViewModel)
      {
         if (ModelState.IsValid)
         {
            var rating = new Rating
            {
               UserId = ratingViewModel.UserId,
               VehicleId = ratingViewModel.VehicleId,
               RatingScore = ratingViewModel.RatingScore,
               Comment = ratingViewModel.Comment
            };

            _context.Ratings.Add(rating);
            var success = await _context.SaveChangesAsync() > 0;

            if (success)
            {
               return RedirectToAction("Details", "Vehicle", new { vehicleId = ratingViewModel.VehicleId });
            }
         }

         return View();
      }

      [HttpPost]
      public async Task<IActionResult> Delete(int ratingId)
      {
         var rating = await _context.Ratings
             .FirstOrDefaultAsync(r => r.Id == ratingId);

         if (rating is null)
         {
            return RedirectToAction("Index", "Vehicle");
         }

         _context.Ratings.Remove(rating);
         var success = await _context.SaveChangesAsync() > 0;

         if (success)
         {
            return RedirectToAction("Details", "Vehicle", new { vehicleId = rating.VehicleId });
         }

         return View();
      }
   }
}