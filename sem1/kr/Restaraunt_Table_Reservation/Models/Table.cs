using System.ComponentModel.DataAnnotations;

namespace RestaurantConsoleApp.Models
{
    public class Table
    {
        public int Id { get; set; }
        
        [Required]
        public int TableNumber { get; set; }
        
        [Required]
        public int Capacity { get; set; }
        
        public string? Description { get; set; }
        
        public TableZone Zone { get; set; }
        
        public bool IsAvailable { get; set; } = true;
        
        public ICollection<Booking> Bookings { get; set; } = new List<Booking>();
    }

    public enum TableZone
    {
        Main,
        Terrace,
        VIP
    }
}
