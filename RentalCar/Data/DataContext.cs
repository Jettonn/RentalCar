using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Models;

namespace Data
{
   public class DataContext : IdentityDbContext<AppUser>
   {
      public DataContext(DbContextOptions<DataContext> options) : base(options)
      { }
   }
}