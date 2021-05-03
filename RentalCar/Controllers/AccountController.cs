using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
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
   public class AccountController : BaseController
   {
      private readonly SignInManager<AppUser> _signInManager;
      private readonly UserManager<AppUser> _userManager;
      private readonly DataContext _context;
      public AccountController(DataContext context,
            UserManager<AppUser> userManager,
            SignInManager<AppUser> signInManager)
      {
         this._context = context;
         this._userManager = userManager;
         this._signInManager = signInManager;
      }

      [HttpGet]
      public IActionResult Register()
      {
         return View();
      }

      [HttpPost]
      public async Task<IActionResult> Register(RegisterViewModel registerViewModel)
      {
         if (ModelState.IsValid)
         {
            var potentialUser = await _context.Users.
                FirstOrDefaultAsync(u => u.Email == registerViewModel.Email);

            if (potentialUser != null)
            {
               ModelState.AddModelError("", "An account already exists with that email address.");
               return View();
            }
            var user = new AppUser
            {
               Name = registerViewModel.Name,
               UserName = registerViewModel.UserName,
               Email = registerViewModel.Email,
            };

            var result = await _userManager.CreateAsync(user, registerViewModel.Password);

            if (result.Succeeded)
            {
               await _signInManager.PasswordSignInAsync(user, registerViewModel.Password, false, false);
               return RedirectToAction("Main", "Home");
            }
         }

         return View();
      }

      [HttpGet]
      public IActionResult Login()
      {
         return View();
      }

      [HttpPost]
      public async Task<IActionResult> Login(LoginViewModel loginViewModel)
      {
         if (ModelState.IsValid)
         {

            var user = await _userManager.FindByEmailAsync(loginViewModel.Email);
            if (user != null)
            {
               var result = await _signInManager.PasswordSignInAsync(user, loginViewModel.Password, false, false);
               if (result.Succeeded)
               {
                  return RedirectToAction("Main", "Home");
               }
            }
         }

         ModelState.AddModelError(string.Empty, "Invalid Email/Password Combination");

         return View();
      }

      [Authorize]
      [HttpPost]
      public async Task<IActionResult> Logout()
      {
         await _signInManager.SignOutAsync();
         return RedirectToAction("Index", "Home");
      }
   }
}