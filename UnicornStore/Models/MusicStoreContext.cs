using System;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using UnicornStore.Configuration;

namespace UnicornStore.Models
{
    public class ApplicationUser : IdentityUser
    {
        public ApplicationUser()
        {
            this.SecurityStamp = Guid.NewGuid().ToString("D"); // Fixes an exception thrown when database is seeded.
        }
    }

    public class UnicornStoreContext : IdentityDbContext<ApplicationUser>
    {
        private readonly DbContextOptionsConfigurator dbContextOptionsConfigurator;

        public UnicornStoreContext(DbContextOptions<UnicornStoreContext> options, DbContextOptionsConfigurator dbContextOptionsConfigurator)
            : base(options)
        {
            this.dbContextOptionsConfigurator = dbContextOptionsConfigurator;

            // TODO: #639
            //ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            this.dbContextOptionsConfigurator.Configure(optionsBuilder);

            base.OnConfiguring(optionsBuilder);
        }

        public DbSet<Blessing> Blessings { get; set; }
        public DbSet<Unicorn> Unicorns { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<Genre> Genres { get; set; }
        public DbSet<CartItem> CartItems { get; set; }
        public DbSet<OrderDetail> OrderDetails { get; set; }
    }
}