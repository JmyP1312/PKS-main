using kr5.Data;
using kr5.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace kr5.Controllers.Api;

[ApiController]
[Route("api/materials")]
public class MaterialsController(ProductionDbContext context) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Get([FromQuery(Name = "low_stock")] bool lowStock = false)
    {
        var query = context.Materials.AsNoTracking();

        if (lowStock)
        {
            query = query.Where(x => x.Quantity <= x.MinimalStock);
        }

        var materials = await query
            .OrderBy(x => x.Name)
            .Select(x => new
            {
                x.Id,
                x.Name,
                x.Quantity,
                Unit = x.UnitOfMeasure,
                MinStock = x.MinimalStock,
                IsLowStock = x.Quantity <= x.MinimalStock
            })
            .ToListAsync();

        return Ok(materials);
    }

    [HttpPost]
    public async Task<IActionResult> Create(MaterialCreateRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
        {
            return BadRequest("Название материала обязательно.");
        }

        var material = new Material
        {
            Name = request.Name.Trim(),
            Quantity = request.Quantity,
            UnitOfMeasure = request.Unit,
            MinimalStock = request.MinStock
        };

        context.Materials.Add(material);
        await context.SaveChangesAsync();

        return CreatedAtAction(nameof(Get), new { id = material.Id }, material);
    }

    [HttpPut("{id:int}/stock")]
    public async Task<IActionResult> UpdateStock(int id, StockUpdateRequest request)
    {
        var material = await context.Materials.FindAsync(id);
        if (material is null)
        {
            return NotFound();
        }

        material.Quantity += request.Amount;
        if (material.Quantity < 0)
        {
            material.Quantity = 0;
        }

        await context.SaveChangesAsync();
        return Ok(new { material.Id, material.Quantity });
    }
}

public record MaterialCreateRequest(string Name, decimal Quantity, string Unit, decimal MinStock);
public record StockUpdateRequest(decimal Amount);
