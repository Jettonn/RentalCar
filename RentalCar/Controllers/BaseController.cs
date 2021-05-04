using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace RentalCar.Controllers
{
   [Route("[controller]/[action]")]
   [Authorize]
   public class BaseController : Controller
   {

   }
}