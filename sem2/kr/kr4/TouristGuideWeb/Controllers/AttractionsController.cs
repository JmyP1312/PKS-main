using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TouristGuideWeb.Data;

namespace TouristGuideWeb.Controllers;

public class AttractionsController : Controller
{
    private readonly TouristDbContext _context;

    public AttractionsController(TouristDbContext context)
    {
        _context = context;
    }

    // GET: /Attractions/Details/5
    public async Task<IActionResult> Details(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var attraction = await _context.Attractions
            .Include(a => a.City)
            .FirstOrDefaultAsync(m => m.Id == id);

        if (attraction == null)
        {
            return NotFound();
        }

        return View(attraction);
    }
}
