using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Data;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Models;

namespace RentalCar
{
   public class Program
   {
      public async static Task Main(string[] args)
      {
         var host = CreateHostBuilder(args).Build();

         var scope = host.Services.CreateScope();
         var serviceProvider = scope.ServiceProvider;

         var userManager = serviceProvider.GetRequiredService<UserManager<AppUser>>();
         var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
         var dbContext = serviceProvider.GetRequiredService<DataContext>();

         dbContext.Database.Migrate();

         await SeedData(userManager, roleManager, dbContext);

         host.Run();
      }

      public static IHostBuilder CreateHostBuilder(string[] args) =>
          Host.CreateDefaultBuilder(args)
              .ConfigureWebHostDefaults(webBuilder =>
              {
                 webBuilder.UseStartup<Startup>();
              });


      private async static Task SeedData(UserManager<AppUser> userManager,
          RoleManager<IdentityRole> roleManager,
          DataContext context)
      {
         if ((await context.Users.ToListAsync()).Count == 0)
         {
            var userOne = new AppUser
            {
               Name = "Shendrit Kqiku",
               Email = "shendritk@test.com",
               UserName = "shendritk"
            };

            var userTwo = new AppUser
            {
               Name = "Jeton Korenica",
               Email = "jetonk@test.com",
               UserName = "jetonk"
            };

            var userThree = new AppUser
            {
               Name = "John Doe",
               Email = "johndoe@test.com",
               UserName = "johndoe"
            };

            await userManager.CreateAsync(userOne, "Pa$$w0rd");
            await userManager.CreateAsync(userTwo, "Pa$$w0rd");
            await userManager.CreateAsync(userThree, "Pa$$w0rd");

            await roleManager.CreateAsync(new IdentityRole { Name = "Admin" });

            var addedUserOne = await userManager.FindByEmailAsync("shendritk@test.com");
            var addedUserTwo = await userManager.FindByEmailAsync("jetonk@test.com");

            await userManager.AddToRoleAsync(addedUserOne, "Admin");
            await userManager.AddToRoleAsync(addedUserTwo, "Admin");

            await context.SaveChangesAsync();
         }
      }
   }
}
