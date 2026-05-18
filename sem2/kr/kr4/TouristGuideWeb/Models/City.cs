using System.ComponentModel.DataAnnotations;

namespace TouristGuideWeb.Models;

public class City
{
    public int Id { get; set; }

    [Required]
    [StringLength(100)]
    public string Name { get; set; } = string.Empty;

    [StringLength(100)]
    public string Region { get; set; } = string.Empty;

    public int Population { get; set; }

    [StringLength(500)]
    public string Description { get; set; } = string.Empty;

    [StringLength(500)]
    public string History { get; set; } = string.Empty;

    [StringLength(200)]
    public string ImageUrl { get; set; } = string.Empty;

    [StringLength(200)]
    public string CoatOfArmsUrl { get; set; } = string.Empty;

    public ICollection<Attraction> Attractions { get; set; } = new List<Attraction>();
}
