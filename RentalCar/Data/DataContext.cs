using System;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Models;

namespace Data
{
   public class DataContext : IdentityDbContext<AppUser>
   {
      public DataContext(DbContextOptions<DataContext> options) : base(options)
      { }

      public DbSet<Vehicle> Vehicles { get; set; }
      public DbSet<Rating> Ratings { get; set; }
      public DbSet<Reservation> Reservations { get; set; }
   }
}