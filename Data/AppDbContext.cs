using Microsoft.EntityFrameworkCore;
using static TTSteelWebAPI.Model.UserClass;

namespace TTSteelWebAPI.Data
{
    public class AppDbContext:DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
         : base(options) { }
        public DbSet<OUSR> OUSR { get; set; }
    }
}
