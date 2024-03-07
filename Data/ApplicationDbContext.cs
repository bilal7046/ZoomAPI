
using Microsoft.EntityFrameworkCore;
using ZoomAPI.Models;

namespace ZoomAPI.Data
{
    public class ApplicationDbContext : DbContext
    {
        public DbSet<Configuration> Configuration { get; set; }
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }
    }
}
