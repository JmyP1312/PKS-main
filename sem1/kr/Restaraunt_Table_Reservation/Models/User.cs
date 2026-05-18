using System.ComponentModel.DataAnnotations;

namespace RestaurantConsoleApp.Models
{
    public class User
    {
        public int Id { get; set; }
        
        [Required]
        public string Username { get; set; } = string.Empty;
        
        [Required]
        public string PasswordHash { get; set; } = string.Empty;
        
        public string? FullName { get; set; }
        
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;
        
        public UserRole Role { get; set; } = UserRole.Client;
        
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        
        public ICollection<Booking> Bookings { get; set; } = new List<Booking>();
    }

    public enum UserRole
    {
        Client,
        Manager,
        Admin
    }
}
