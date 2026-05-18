using Microsoft.EntityFrameworkCore;
using RestaurantConsoleApp.Models;

namespace RestaurantConsoleApp.Data
{
    public class RestaurantDbContext : DbContext
    {
        public DbSet<User> Users { get; set; }
        public DbSet<Table> Tables { get; set; }
        public DbSet<Booking> Bookings { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlite("Data Source=restaurant.db");
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Конфигурация связей
            modelBuilder.Entity<Booking>()
                .HasOne(b => b.User)
                .WithMany(u => u.Bookings)
                .HasForeignKey(b => b.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Booking>()
                .HasOne(b => b.Table)
                .WithMany(t => t.Bookings)
                .HasForeignKey(b => b.TableId)
                .OnDelete(DeleteBehavior.Cascade);

            // Начальные данные для столиков
            modelBuilder.Entity<Table>().HasData(
                new Table { Id = 1, TableNumber = 1, Capacity = 2, Zone = TableZone.Main, Description = "Столик на 2 персоны", IsAvailable = true },
                new Table { Id = 2, TableNumber = 2, Capacity = 4, Zone = TableZone.Main, Description = "Столик на 4 персоны", IsAvailable = true },
                new Table { Id = 3, TableNumber = 3, Capacity = 6, Zone = TableZone.Main, Description = "Столик на 6 персон", IsAvailable = true },
                new Table { Id = 4, TableNumber = 4, Capacity = 4, Zone = TableZone.Terrace, Description = "Столик на террасе", IsAvailable = true },
                new Table { Id = 5, TableNumber = 5, Capacity = 8, Zone = TableZone.VIP, Description = "VIP столик на 8 персон", IsAvailable = true }
            );

            // Создаём администратора по умолчанию
            modelBuilder.Entity<User>().HasData(
                new User
                {
                    Id = 1,
                    Username = "admin",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("admin123"),
                    Email = "admin@restaurant.com",
                    FullName = "Администратор",
                    Role = UserRole.Admin,
                    CreatedAt = DateTime.Now
                }
            );
        }
    }
}
