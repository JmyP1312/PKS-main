using RestaurantConsoleApp.Models;

namespace RestaurantConsoleApp.Services
{
    public static class AuthService
    {
        public static User? CurrentUser { get; private set; }

        public static bool Login(string username, string password)
        {
            using var db = new Data.RestaurantDbContext();
            var user = db.Users.FirstOrDefault(u => u.Username == username);

            if (user != null && BCrypt.Net.BCrypt.Verify(password, user.PasswordHash))
            {
                CurrentUser = user;
                return true;
            }

            return false;
        }

        public static bool Register(string username, string password, string email, string? fullName)
        {
            using var db = new Data.RestaurantDbContext();

            // Проверяем, существует ли пользователь
            if (db.Users.Any(u => u.Username == username || u.Email == email))
            {
                return false;
            }

            var user = new User
            {
                Username = username,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(password),
                Email = email,
                FullName = fullName,
                Role = UserRole.Client,
                CreatedAt = DateTime.Now
            };

            db.Users.Add(user);
            db.SaveChanges();
            return true;
        }

        public static void Logout()
        {
            CurrentUser = null;
        }

        public static bool IsLoggedIn => CurrentUser != null;
        public static bool IsAdmin => CurrentUser?.Role == UserRole.Admin;
        public static bool IsManager => CurrentUser?.Role == UserRole.Manager || IsAdmin;
    }
}
