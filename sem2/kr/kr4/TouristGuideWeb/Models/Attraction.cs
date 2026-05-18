using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TouristGuideWeb.Models;

public class Attraction
{
    public int Id { get; set; }

    [Required]
    [StringLength(100)]
    public string Name { get; set; } = string.Empty;

    [StringLength(500)]
    public string Description { get; set; } = string.Empty;

    [StringLength(1000)]
    public string History { get; set; } = string.Empty;

    [StringLength(200)]
    public string ImageUrl { get; set; } = string.Empty;

    [StringLength(100)]
    public string WorkingHours { get; set; } = string.Empty;

    public decimal? TicketPrice { get; set; }

    public int CityId { get; set; }

    [ForeignKey("CityId")]
    public City? City { get; set; }
}
