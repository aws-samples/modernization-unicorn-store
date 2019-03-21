using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace UnicornStore.Models
{
    public class ApplicationUser : IdentityUser { }

    public class UnicornStoreContext : IdentityDbContext<ApplicationUser>
    {
        public UnicornStoreContext(DbContextOptions<UnicornStoreContext> options)
            : base(options)
        {
            // TODO: #639
            //ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;
        }

        public DbSet<Blessing> Blessings { get; set; }
        public DbSet<Unicorn> Unicorns { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<Genre> Genres { get; set; }
        public DbSet<CartItem> CartItems { get; set; }
        public DbSet<OrderDetail> OrderDetails { get; set; }
    }
}