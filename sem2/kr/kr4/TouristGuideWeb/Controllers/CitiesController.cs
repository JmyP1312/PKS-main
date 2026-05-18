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
        var cities = from c in _context.Cities select c;

        if (!String.IsNullOrEmpty(searchString))
        {
            cities = cities.Where(c => c.Name.Contains(searchString));
        }

        return View(await cities.ToListAsync());
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
