using kr5.Data;
using kr5.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text.RegularExpressions;

namespace kr5.Controllers.Api;

[ApiController]
[Route("api/materials")]
public class MaterialsController(ProductionDbContext context) : ControllerBase
{
    private static readonly string[] AllowedUnits = new[] { "кг", "шт", "м", "л", "м2", "м3", "комплект" };

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

        var name = request.Name.Trim();
        if (!Regex.IsMatch(name, @"^[\p{L}0-9 \-]+$"))
        {
            return BadRequest("Название материала должно содержать только буквы, цифры, пробелы и дефисы.");
        }

        if (await context.Materials.AnyAsync(x => x.Name == name))
        {
            return BadRequest("Материал с таким названием уже существует.");
        }

        var unit = request.Unit?.Trim();
        if (string.IsNullOrWhiteSpace(unit) || !AllowedUnits.Contains(unit))
        {
            return BadRequest($"Единица измерения должна быть одной из: {string.Join(", ", AllowedUnits)}.");
        }

        if (request.Quantity < 0)
        {
            return BadRequest("Количество не может быть отрицательным.");
        }

        if (request.MinStock < 0)
        {
            return BadRequest("Мин. запас не может быть отрицательным.");
        }

        var material = new Material
        {
            Name = name,
            Quantity = request.Quantity,
            UnitOfMeasure = unit,
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

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var material = await context.Materials
            .Include(x => x.ProductMaterials)
            .FirstOrDefaultAsync(x => x.Id == id);

        if (material is null)
        {
            return NotFound();
        }

        if (material.ProductMaterials.Any())
        {
            return BadRequest("Нельзя удалить материал, который используется в продуктах.");
        }

        context.Materials.Remove(material);
        await context.SaveChangesAsync();
        return NoContent();
    }
}

public record MaterialCreateRequest(string Name, decimal Quantity, string Unit, decimal MinStock);
public record StockUpdateRequest(decimal Amount);
