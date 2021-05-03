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
   public class RatingsController : Controller
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
               return RedirectToAction("Index");
            }
         }

         return View();
      }

      [HttpPost("{ratingId}")]
      public async Task<IActionResult> Delete(int ratingId)
      {
         var rating = await _context.Ratings
             .FirstOrDefaultAsync(r => r.Id == ratingId);

         if (rating is null)
         {
            return RedirectToAction("Index");
         }

         _context.Ratings.Remove(rating);
         var success = await _context.SaveChangesAsync() > 0;

         if (success)
         {
            return RedirectToAction("Index");
         }

         return View();
      }
   }
}