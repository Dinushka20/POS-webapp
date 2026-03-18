using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using POS.Api.Models;

namespace POS.Api.Data
{
    public class AppDbContext : IdentityDbContext<AppUser>
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<Branch> Branches { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<Stock> Stocks { get; set; }
        public DbSet<StockAuditLog> StockAuditLogs { get; set; }
        public DbSet<Customer> Customers { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderItem> OrderItems { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Decimal precision (18,2) for all decimal columns
            builder.Entity<Product>(e =>
            {
                e.Property(p => p.Price).HasPrecision(18, 2);
                e.Property(p => p.CostPrice).HasPrecision(18, 2);
                e.HasIndex(p => p.Barcode).IsUnique();
            });

            builder.Entity<Order>(e =>
            {
                e.Property(o => o.TotalAmount).HasPrecision(18, 2);
                e.Property(o => o.DiscountAmount).HasPrecision(18, 2);
                e.Property(o => o.CashAmount).HasPrecision(18, 2);
                e.Property(o => o.CardAmount).HasPrecision(18, 2);

                // Restrict delete on Order → Branch
                e.HasOne(o => o.Branch)
                    .WithMany(b => b.Orders)
                    .HasForeignKey(o => o.BranchId)
                    .OnDelete(DeleteBehavior.Restrict);

                // Cascade delete on Order → OrderItems
                e.HasMany(o => o.Items)
                    .WithOne(oi => oi.Order)
                    .HasForeignKey(oi => oi.OrderId)
                    .OnDelete(DeleteBehavior.Cascade);

                e.HasOne(o => o.User)
                    .WithMany()
                    .HasForeignKey(o => o.UserId)
                    .OnDelete(DeleteBehavior.Restrict);

                e.HasOne(o => o.Customer)
                    .WithMany(c => c.Orders)
                    .HasForeignKey(o => o.CustomerId)
                    .OnDelete(DeleteBehavior.SetNull);
            });

            builder.Entity<OrderItem>(e =>
            {
                e.Property(oi => oi.UnitPrice).HasPrecision(18, 2);
                e.Property(oi => oi.DiscountAmount).HasPrecision(18, 2);
            });

            builder.Entity<Customer>(e =>
            {
                e.HasIndex(c => c.Phone).IsUnique();
            });

            builder.Entity<Stock>(e =>
            {
                e.HasOne(s => s.Product)
                    .WithMany(p => p.Stocks)
                    .HasForeignKey(s => s.ProductId)
                    .OnDelete(DeleteBehavior.Cascade);

                e.HasOne(s => s.Branch)
                    .WithMany(b => b.Stocks)
                    .HasForeignKey(s => s.BranchId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            builder.Entity<StockAuditLog>(e =>
            {
                e.HasOne(s => s.Product)
                    .WithMany()
                    .HasForeignKey(s => s.ProductId)
                    .OnDelete(DeleteBehavior.Restrict);

                e.HasOne(s => s.Branch)
                    .WithMany()
                    .HasForeignKey(s => s.BranchId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            builder.Entity<AppUser>(e =>
            {
                e.HasOne(u => u.Branch)
                    .WithMany(b => b.Users)
                    .HasForeignKey(u => u.BranchId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // Seed data
            builder.Entity<Branch>().HasData(
                new Branch { Id = 1, Name = "Main Branch", Address = "Colombo", IsActive = true, CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
                new Branch { Id = 2, Name = "Branch 2", Address = "Kandy", IsActive = true, CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc) }
            );

            builder.Entity<Category>().HasData(
                new Category { Id = 1, Name = "Electronics" },
                new Category { Id = 2, Name = "Beverages" },
                new Category { Id = 3, Name = "Snacks" }
            );
        }
    }
}
