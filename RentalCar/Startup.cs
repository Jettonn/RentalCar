using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Data;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Models;

namespace RentalCar
{
   public class Startup
   {
      public Startup(IConfiguration configuration)
      {
         Configuration = configuration;
      }

      public IConfiguration Configuration { get; }

      public void ConfigureServices(IServiceCollection services)
      {
         services.AddControllersWithViews();

         services.AddDbContext<DataContext>(options =>
         options.UseMySql(Configuration["DBInfo:ConnectionString"]));

         services.AddIdentity<AppUser, IdentityRole>(options =>
         {
            options.Password.RequiredLength = 8;
         })
          .AddEntityFrameworkStores<DataContext>()
          .AddDefaultTokenProviders();

         services.ConfigureApplicationCookie(opts =>
             {
                opts.LoginPath = "/Account/Login";
                opts.Cookie.Name = "RentalCarAuth";
             });
      }

      public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
      {
         if (env.IsDevelopment())
         {
            app.UseDeveloperExceptionPage();
         }
         else
         {
            app.UseExceptionHandler("/Home/Error");
         }
         app.UseStaticFiles();

         app.UseRouting();

         app.UseAuthorization();

         app.UseEndpoints(endpoints =>
         {
            endpoints.MapControllerRoute(
                   name: "default",
                   pattern: "{controller=Home}/{action=Index}/{id?}");
         });
      }
   }
}
