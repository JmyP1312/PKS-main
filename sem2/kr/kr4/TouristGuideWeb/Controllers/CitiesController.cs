using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TouristGuideWeb.Data;
using TouristGuideWeb.Models;

namespace TouristGuideWeb.Controllers;

public class CitiesController : Controller
{
    private readonly TouristDbContext _context;

    public CitiesController(TouristDbContext context)
    {
        _context = context;
    }

    // GET: /Cities
    public async Task<IActionResult> Index(string? searchString)
    {
        ViewData["CurrentFilter"] = searchString;

        var cities = _context.Cities.AsQueryable();

        if (!string.IsNullOrWhiteSpace(searchString))
        {
            var query = searchString.Trim();
            cities = cities.Where(c =>
                EF.Functions.Like(c.Name, $"%{query}%") ||
                EF.Functions.Like(c.Region, $"%{query}%") ||
                EF.Functions.Like(c.Description, $"%{query}%"));
        }

        return View(await cities.OrderBy(c => c.Name).ToListAsync());
    }

    // GET: /Cities/Details/5
    public async Task<IActionResult> Details(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var city = await _context.Cities
            .Include(c => c.Attractions)
            .FirstOrDefaultAsync(m => m.Id == id);

        if (city == null)
        {
            return NotFound();
        }

        return View(city);
    }
}
