using System.ComponentModel.DataAnnotations;

namespace RestaurantConsoleApp.Models
{
    public class Booking
    {
        public int Id { get; set; }
        
        [Required]
        public int UserId { get; set; }
        public User? User { get; set; }
        
        [Required]
        public int TableId { get; set; }
        public Table? Table { get; set; }
        
        [Required]
        public DateTime BookingDate { get; set; }
        
        [Required]
        public TimeSpan StartTime { get; set; }
        
        [Required]
        public TimeSpan EndTime { get; set; }
        
        [Required]
        public int GuestCount { get; set; }
        
        public string? SpecialRequests { get; set; }
        
        public BookingStatus Status { get; set; } = BookingStatus.Pending;
        
        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }

    public enum BookingStatus
    {
        Pending,
        Confirmed,
        Cancelled,
        Completed
    }
}
